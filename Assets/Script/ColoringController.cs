using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class ColoringController : MonoBehaviour
{
    [Header("Основные элементы")]
    public TMP_Text questionText;
    public Button[] answerButtons;
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public TMP_Text victoryText;
    public TMP_Text difficultyText;
    public TMP_Text levelProgressText;

    [Header("Части рисунка")]
    public GameObject[] correctParts;
    public GameObject[] wrongParts;
    public Image baseDrawing;

    [Header("Настройки")]
    public int maxErrors = 2;
    public string levelId = "Coloring_Level_1";

    [Header("Система сложности")]
    public ProgressiveLevelManager progressiveLevelManager;

    private int currentQuestion = 0;
    private int correctAnswer;
    private bool isGameActive = true;
    private int errorCount = 0;
    private int correctAnswers = 0;
    private int currentDifficultyLevel = 0;
    private AdaptiveDifficulty adaptiveDifficulty = new AdaptiveDifficulty();
    private DifficultySettings currentDifficultySettings;

    private bool isProcessingAnswer = false;
    private Coroutine currentAnimationCoroutine;

    void Start()
    {
        if (string.IsNullOrEmpty(levelId))
        {
            levelId = "Coloring_Level_1";
        }

        currentDifficultySettings = progressiveLevelManager.GetDifficultyForLevel(levelId);
        currentDifficultyLevel = progressiveLevelManager.ExtractLevelNumber(levelId) - 1;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayColoringGameMusic();
        }

        InitializeDrawing();
        UpdateDifficultyDisplay();
        GenerateQuestion();

        Debug.Log($"Загружен уровень раскраски: {levelId}, Уровень сложности: {currentDifficultyLevel + 1}");
    }

    void Update()
    {
        // ===== ПК УПРАВЛЕНИЕ =====
        // Клавиши 1-4 для ответов
        if (Input.GetKeyDown(KeyCode.Alpha1) && answerButtons.Length > 0 && answerButtons[0].interactable)
            answerButtons[0].onClick.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha2) && answerButtons.Length > 1 && answerButtons[1].interactable)
            answerButtons[1].onClick.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha3) && answerButtons.Length > 2 && answerButtons[2].interactable)
            answerButtons[2].onClick.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha4) && answerButtons.Length > 3 && answerButtons[3].interactable)
            answerButtons[3].onClick.Invoke();

        // R - рестарт
        if (Input.GetKeyDown(KeyCode.R))
            RestartLevel();

        // Escape - выход в меню
        if (Input.GetKeyDown(KeyCode.Escape))
            BackToMenu();

        // Space - продолжить
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (victoryPanel.activeSelf)
            {
                Button nextBtn = victoryPanel.GetComponentInChildren<Button>();
                if (nextBtn != null) nextBtn.onClick.Invoke();
            }
            else if (defeatPanel.activeSelf)
            {
                Button restartBtn = defeatPanel.GetComponentInChildren<Button>();
                if (restartBtn != null) restartBtn.onClick.Invoke();
            }
        }
    }

    void InitializeDrawing()
    {
        foreach (GameObject part in correctParts)
        {
            if (part != null) part.SetActive(false);
        }
        foreach (GameObject part in wrongParts)
        {
            if (part != null) part.SetActive(false);
        }

        if (baseDrawing != null) baseDrawing.gameObject.SetActive(true);
    }

    void UpdateDifficultyDisplay()
    {
        if (difficultyText != null)
        {
            difficultyText.text = $"Уровень {currentDifficultyLevel + 1}";
            difficultyText.color = GetDifficultyColor(currentDifficultyLevel);
        }

        if (levelProgressText != null)
        {
            levelProgressText.text = $"Вопрос: {currentQuestion + 1}/{currentDifficultySettings.totalQuestions}";
        }
    }

    Color GetDifficultyColor(int level)
    {
        switch (level)
        {
            case 0: case 1: return Color.green;
            case 2: case 3: return Color.yellow;
            case 4: case 5: return new Color(1f, 0.5f, 0f);
            case 6: return Color.red;
            case 7: return Color.magenta;
            default: return Color.white;
        }
    }

    void GenerateQuestion()
    {
        if (!isGameActive) return;

        int number1, number2;

        int[] weakNumbers = adaptiveDifficulty.GetWeakNumbers();
        bool useWeakNumber = weakNumbers.Length > 0 && Random.Range(0f, 1f) > 0.7f;

        if (useWeakNumber && weakNumbers.Length > 0)
        {
            number1 = weakNumbers[Random.Range(0, weakNumbers.Length)];
            number2 = Random.Range(currentDifficultySettings.minNumber, currentDifficultySettings.maxNumber + 1);
        }
        else
        {
            number1 = Random.Range(currentDifficultySettings.minNumber, currentDifficultySettings.maxNumber + 1);
            number2 = Random.Range(currentDifficultySettings.minNumber, currentDifficultySettings.maxNumber + 1);
        }

        correctAnswer = number1 * number2;
        questionText.text = $"{number1} × {number2} = ?";

        GenerateAnswers();
    }

    void GenerateAnswers()
    {
        int[] answers = new int[4];
        int correctIndex = Random.Range(0, 4);

        for (int i = 0; i < 4; i++)
        {
            if (i == correctIndex)
            {
                answers[i] = correctAnswer;
            }
            else
            {
                answers[i] = GenerateWrongAnswer(answers, i);
            }

            answerButtons[i].GetComponentInChildren<TMP_Text>().text = answers[i].ToString();

            int index = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index, answers[index]));
        }
    }

    int GenerateWrongAnswer(int[] existingAnswers, int currentIndex)
    {
        int wrongAnswer;
        int attempts = 0;
        int maxAttempts = 15;

        do
        {
            if (currentDifficultySettings.useTrickyAnswers && attempts < 3)
            {
                wrongAnswer = GenerateTrickyAnswer();
            }
            else
            {
                int variation = Random.Range(-currentDifficultySettings.wrongAnswerRange,
                                           currentDifficultySettings.wrongAnswerRange + 1);
                wrongAnswer = correctAnswer + variation;
            }

            attempts++;

            if (attempts > maxAttempts)
            {
                wrongAnswer = correctAnswer + (currentIndex + 1) * (currentIndex % 2 == 0 ? 1 : -1);
                Debug.LogWarning($"Превышено количество попыток генерации неправильного ответа. Использовано аварийное значение: {wrongAnswer}");
                break;
            }

        } while (wrongAnswer == correctAnswer || wrongAnswer <= 0 ||
                 System.Array.Exists(existingAnswers, x => x == wrongAnswer));

        return wrongAnswer;
    }

    int GenerateTrickyAnswer()
    {
        int trickyType = Random.Range(0, 3);

        switch (trickyType)
        {
            case 0:
                int neighbor1 = Random.Range(currentDifficultySettings.minNumber, currentDifficultySettings.maxNumber + 1);
                int neighbor2 = Random.Range(currentDifficultySettings.minNumber, currentDifficultySettings.maxNumber + 1);
                return neighbor1 * neighbor2;

            case 1:
                string correctStr = correctAnswer.ToString();
                if (correctStr.Length == 2)
                {
                    try
                    {
                        char[] chars = correctStr.ToCharArray();
                        System.Array.Reverse(chars);
                        return int.Parse(new string(chars));
                    }
                    catch
                    {
                        return correctAnswer + 1;
                    }
                }
                break;

            case 2:
                int a = correctAnswer / 10;
                int b = correctAnswer % 10;
                if (a > 0 && b > 0) return a + b;
                break;
        }

        return correctAnswer + Random.Range(-currentDifficultySettings.wrongAnswerRange,
                                          currentDifficultySettings.wrongAnswerRange + 1);
    }

    public void OnAnswerSelected(int buttonIndex, int selectedAnswer)
    {
        if (!isGameActive || isProcessingAnswer) return;

        isProcessingAnswer = true;

        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }

        if (selectedAnswer == correctAnswer)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("ColoringCorrectAnswer");
                AudioManager.Instance.PlaySFX("ColorAppear");
            }

            correctAnswers++;
            currentAnimationCoroutine = StartCoroutine(ShowPartWithAnimation(currentQuestion, true));
        }
        else
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("ColoringWrongAnswer");
            }

            errorCount++;

            if (TryParseQuestion(questionText.text, out int num1, out int num2))
            {
                adaptiveDifficulty.RecordMistake(num1, num2);
            }

            currentAnimationCoroutine = StartCoroutine(ShowPartWithAnimation(currentQuestion, false));
        }

        currentQuestion++;
        StartCoroutine(CheckGameEndWithDelay());
    }

    private bool TryParseQuestion(string questionText, out int num1, out int num2)
    {
        num1 = 0;
        num2 = 0;

        try
        {
            string[] parts = questionText.Split('×');
            if (parts.Length != 2) return false;

            string[] secondPart = parts[1].Split('=');
            if (secondPart.Length == 0) return false;

            if (!int.TryParse(parts[0].Trim(), out num1)) return false;
            if (!int.TryParse(secondPart[0].Trim(), out num2)) return false;

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка парсинга вопроса: {e.Message}");
            return false;
        }
    }

    IEnumerator ShowPartWithAnimation(int partIndex, bool isCorrect)
    {
        ShowPart(partIndex, isCorrect);
        yield return new WaitForSeconds(0.6f);
        isProcessingAnswer = false;
    }

    void ShowPart(int partIndex, bool isCorrect)
    {
        if (partIndex < correctParts.Length && partIndex < wrongParts.Length)
        {
            if (isCorrect)
            {
                if (correctParts[partIndex] != null)
                {
                    correctParts[partIndex].SetActive(true);
                    StartCoroutine(AnimateAppearance(correctParts[partIndex]));
                }
                if (wrongParts[partIndex] != null)
                    wrongParts[partIndex].SetActive(false);
            }
            else
            {
                if (wrongParts[partIndex] != null)
                {
                    wrongParts[partIndex].SetActive(true);
                    StartCoroutine(AnimateAppearance(wrongParts[partIndex]));
                }
                if (correctParts[partIndex] != null)
                    correctParts[partIndex].SetActive(false);
            }
        }
    }

    IEnumerator AnimateAppearance(GameObject part)
    {
        if (part == null || !isGameActive) yield break;

        Image partImage = part.GetComponent<Image>();
        if (partImage != null)
        {
            float duration = 0.5f;
            float elapsed = 0f;
            Color originalColor = partImage.color;
            Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

            partImage.color = transparentColor;

            while (elapsed < duration && isGameActive)
            {
                partImage.color = Color.Lerp(transparentColor, originalColor, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (isGameActive)
            {
                partImage.color = originalColor;
            }
        }
    }

    IEnumerator CheckGameEndWithDelay()
    {
        yield return new WaitForSeconds(0.3f);

        if (errorCount >= maxErrors)
        {
            Defeat();
        }
        else if (currentQuestion >= currentDifficultySettings.totalQuestions)
        {
            if (errorCount == 0)
            {
                Victory(true);
            }
            else if (errorCount <= 2)
            {
                Victory(false);
            }
            else
            {
                Defeat();
            }
        }
        else
        {
            GenerateQuestion();
        }
    }

    void Victory(bool isPerfect)
    {
        if (!isGameActive) return;

        isGameActive = false;

        SaveLevelProgress(isPerfect);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayVictoryMusic();
            AudioManager.Instance.PlaySFX("DrawingComplete");
            if (isPerfect)
            {
                AudioManager.Instance.PlaySFX("StarAppear");
            }
        }

        if (isPerfect)
        {
            UnlockGalleryImage();
            victoryText.text = "Идеально! \nРисунок добавлен в галерею!";
        }
        else
        {
            victoryText.text = $"Неплохо!\nПравильных ответов: {correctAnswers}/{currentDifficultySettings.totalQuestions}";
        }

        victoryPanel.SetActive(true);
    }

    void Defeat()
    {
        if (!isGameActive) return;

        isGameActive = false;

        SaveLevelProgress(false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDefeatMusic();
        }

        TMP_Text defeatText = defeatPanel.GetComponentInChildren<TMP_Text>();
        if (defeatText != null)
        {
            if (errorCount >= maxErrors)
            {
                defeatText.text = "Слишком много ошибок!\nПопробуй еще раз!";
            }
            else
            {
                defeatText.text = $"Почти получилось!\nПравильных ответов: {correctAnswers}/{currentDifficultySettings.totalQuestions}";
            }
        }

        defeatPanel.SetActive(true);
    }

    void SaveLevelProgress(bool isPerfect)
    {
        try
        {
            int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);

            string completedKey = $"Slot_{currentSlot}_{levelId}_completed";
            PlayerPrefs.SetInt(completedKey, 1);

            int baseStars = isPerfect ? 3 : (correctAnswers >= currentDifficultySettings.totalQuestions * 0.7f ? 2 : 1);
            int difficultyBonus = (currentDifficultyLevel >= 5 && correctAnswers >= currentDifficultySettings.totalQuestions * 0.8f) ? 1 : 0;
            int totalStars = Mathf.Min(3, baseStars + difficultyBonus);

            string starsKey = $"Slot_{currentSlot}_{levelId}_stars";
            PlayerPrefs.SetInt(starsKey, totalStars);

            PlayerPrefs.Save();

            Debug.Log($"Сохранен прогресс: {levelId}, Звезд: {totalStars}, Уровень: {currentDifficultyLevel + 1}, Идеально: {isPerfect}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка сохранения прогресса: {e.Message}");
        }
    }

    void UnlockGalleryImage()
    {
        try
        {
            int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);

            int levelNumber = progressiveLevelManager.ExtractLevelNumber(levelId);

            string galleryKey = $"Slot_{currentSlot}_Gallery_Level_{levelNumber}";
            PlayerPrefs.SetInt(galleryKey, 1);
            PlayerPrefs.Save();

            Debug.Log($"Рисунок уровня {levelNumber} разблокирован в галерее слота {currentSlot}!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка разблокировки галереи: {e.Message}");
        }
    }

    public void RestartLevel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        StopAllCoroutines();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        StopAllCoroutines();
        SceneManager.LoadScene(2);
    }

    public void LoadNextLevel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        int currentLevel = progressiveLevelManager.ExtractLevelNumber(levelId);
        int nextLevel = currentLevel + 1;

        if (nextLevel <= 4)
        {
            string nextSceneName = $"ColoringLevel{nextLevel}";
            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                StopAllCoroutines();
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                BackToMenu();
            }
        }
        else
        {
            BackToMenu();
        }
    }

    public void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    public void PlayHoverSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonHover();
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}