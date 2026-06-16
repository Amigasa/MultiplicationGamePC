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

    // ЗАЩИТА ОТ ЗАВИСАНИЙ
    private bool isProcessingAnswer = false;
    private Coroutine currentAnimationCoroutine;

    void Start()
    {
        if (string.IsNullOrEmpty(levelId))
        {
            levelId = "Coloring_Level_1";
        }

        // Получаем настройки сложности из ProgressiveLevelManager
        currentDifficultySettings = progressiveLevelManager.GetDifficultyForLevel(levelId);

        // Определяем текущий уровень сложности на основе levelId
        currentDifficultyLevel = progressiveLevelManager.ExtractLevelNumber(levelId) - 1;

        // Воспроизводим музыку для режима раскраски
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayColoringGameMusic();
        }

        InitializeDrawing();
        UpdateDifficultyDisplay();
        GenerateQuestion();

        Debug.Log($"Загружен уровень раскраски: {levelId}, Уровень сложности: {currentDifficultyLevel + 1}");
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
            case 4: case 5: return new Color(1f, 0.5f, 0f); // оранжевый
            case 6: return Color.red;
            case 7: return Color.magenta;
            default: return Color.white;
        }
    }

    void GenerateQuestion()
    {
        if (!isGameActive) return;

        int number1, number2;

        // Используем адаптивную сложность: иногда показываем слабые числа
        int[] weakNumbers = adaptiveDifficulty.GetWeakNumbers();
        bool useWeakNumber = weakNumbers.Length > 0 && Random.Range(0f, 1f) > 0.7f;

        if (useWeakNumber && weakNumbers.Length > 0)
        {
            // Используем одно из слабых чисел
            number1 = weakNumbers[Random.Range(0, weakNumbers.Length)];
            number2 = Random.Range(currentDifficultySettings.minNumber, currentDifficultySettings.maxNumber + 1);
        }
        else
        {
            // Обычная генерация чисел
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
        int maxAttempts = 15; // ЗАЩИТА ОТ БЕСКОНЕЧНОГО ЦИКЛА

        do
        {
            if (currentDifficultySettings.useTrickyAnswers && attempts < 3)
            {
                // Генерируем "хитрые" неправильные ответы
                wrongAnswer = GenerateTrickyAnswer();
            }
            else
            {
                // Обычные неправильные ответы
                int variation = Random.Range(-currentDifficultySettings.wrongAnswerRange,
                                           currentDifficultySettings.wrongAnswerRange + 1);
                wrongAnswer = correctAnswer + variation;
            }

            attempts++;

            if (attempts > maxAttempts)
            {
                // АВАРИЙНЫЙ ВЫХОД ИЗ ЦИКЛА
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
        // Генерируем "хитрые" ответы, которые могут сбить с толку
        int trickyType = Random.Range(0, 3);

        switch (trickyType)
        {
            case 0: // Ответ от соседнего примера
                int neighbor1 = Random.Range(currentDifficultySettings.minNumber, currentDifficultySettings.maxNumber + 1);
                int neighbor2 = Random.Range(currentDifficultySettings.minNumber, currentDifficultySettings.maxNumber + 1);
                return neighbor1 * neighbor2;

            case 1: // Перепутанные цифры (например, 24 вместо 42)
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
                        // Если что-то пошло не так, используем fallback
                        return correctAnswer + 1;
                    }
                }
                break;

            case 2: // Сложение вместо умножения
                int a = correctAnswer / 10;
                int b = correctAnswer % 10;
                if (a > 0 && b > 0) return a + b;
                break;
        }

        // Fallback - обычный неправильный ответ
        return correctAnswer + Random.Range(-currentDifficultySettings.wrongAnswerRange,
                                          currentDifficultySettings.wrongAnswerRange + 1);
    }

    public void OnAnswerSelected(int buttonIndex, int selectedAnswer)
    {
        // ЗАЩИТА ОТ МНОЖЕСТВЕННЫХ НАЖАТИЙ
        if (!isGameActive || isProcessingAnswer) return;

        isProcessingAnswer = true;

        // ОСТАНАВЛИВАЕМ ПРЕДЫДУЩУЮ АНИМАЦИЮ
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }

        if (selectedAnswer == correctAnswer)
        {
            // Звуки правильного ответа
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
            // Звуки неправильного ответа
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("ColoringWrongAnswer");
            }

            errorCount++;

            // ЗАЩИЩЕННЫЙ ПАРСИНГ ВОПРОСА
            if (TryParseQuestion(questionText.text, out int num1, out int num2))
            {
                adaptiveDifficulty.RecordMistake(num1, num2);
            }

            currentAnimationCoroutine = StartCoroutine(ShowPartWithAnimation(currentQuestion, false));
        }

        currentQuestion++;

        // ПРОВЕРЯЕМ УСЛОВИЯ ОКОНЧАНИЯ ИГРЫ С ЗАДЕРЖКОЙ
        StartCoroutine(CheckGameEndWithDelay());
    }

    // ЗАЩИЩЕННЫЙ ПАРСИНГ ВОПРОСА
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

        // ЖДЕМ ЗАВЕРШЕНИЯ АНИМАЦИИ ПЕРЕД РАЗБЛОКИРОВКОЙ
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

            while (elapsed < duration && isGameActive) // ЗАЩИТА
            {
                partImage.color = Color.Lerp(transparentColor, originalColor, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (isGameActive) // ЗАЩИТА
            {
                partImage.color = originalColor;
            }
        }
    }

    IEnumerator CheckGameEndWithDelay()
    {
        // КОРОТКАЯ ЗАДЕРЖКА ПЕРЕД ПРОВЕРКОЙ КОНЦА ИГРЫ
        yield return new WaitForSeconds(0.3f);

        if (errorCount >= maxErrors)
        {
            Defeat();
        }
        else if (currentQuestion >= currentDifficultySettings.totalQuestions)
        {
            // Определяем результат на основе количества ошибок
            if (errorCount == 0)
            {
                Victory(true); // Идеально - 0 ошибок
            }
            else if (errorCount <= 2)
            {
                Victory(false); // Неплохо - 1-2 ошибки
            }
            else
            {
                Defeat(); // Поражение - 3-4 ошибки
            }
        }
        else
        {
            GenerateQuestion();
        }
    }

    void Victory(bool isPerfect)
    {
        if (!isGameActive) return; // ЗАЩИТА ОТ ПОВТОРНЫХ ВЫЗОВОВ

        isGameActive = false;

        SaveLevelProgress(isPerfect);

        // Звуки победы
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayVictoryMusic();
            AudioManager.Instance.PlaySFX("DrawingComplete");

            // Дополнительный звук для идеального результата
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
            // 1-2 ошибки
            victoryText.text = $"Неплохо!\nПравильных ответов: {correctAnswers}/{currentDifficultySettings.totalQuestions}";
        }

        victoryPanel.SetActive(true);
    }

    void Defeat()
    {
        if (!isGameActive) return; // ЗАЩИТА ОТ ПОВТОРНЫХ ВЫЗОВОВ

        isGameActive = false;

        SaveLevelProgress(false);

        // Звуки поражения
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDefeatMusic();
        }

        TMP_Text defeatText = defeatPanel.GetComponentInChildren<TMP_Text>();
        if (defeatText != null)
        {
            if (errorCount >= maxErrors)
            {
                // Слишком много ошибок (maxErrors и больше)
                defeatText.text = "Слишком много ошибок!\nПопробуй еще раз!";
            }
            else
            {
                // 3-4 ошибки
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

            // Сохраняем что уровень пройден
            string completedKey = $"Slot_{currentSlot}_{levelId}_completed";
            PlayerPrefs.SetInt(completedKey, 1);

            // Расчет звезд
            int baseStars = isPerfect ? 3 : (correctAnswers >= currentDifficultySettings.totalQuestions * 0.7f ? 2 : 1);

            // Бонус за высокий уровень сложности
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

    // Кнопки UI
    public void RestartLevel()
    {
        // Звук кнопки перезапуска
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // ОСТАНАВЛИВАЕМ ВСЕ КОРУТИНЫ
        StopAllCoroutines();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        // Звук кнопки возврата
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // ОСТАНАВЛИВАЕМ ВСЕ КОРУТИНЫ
        StopAllCoroutines();

        SceneManager.LoadScene(2);
    }

    public void LoadNextLevel()
    {
        // Звук кнопки следующего уровня
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        int currentLevel = progressiveLevelManager.ExtractLevelNumber(levelId);
        int nextLevel = currentLevel + 1;

        if (nextLevel <= 4) // У вас 4 уровня раскраски
        {
            string nextSceneName = $"ColoringLevel{nextLevel}";
            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                // ОСТАНАВЛИВАЕМ ВСЕ КОРУТИНЫ
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

    // Дополнительные методы для управления звуками
    public void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }

    public void PlayHoverSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonHover();
        }
    }

    void OnDestroy()
    {
        // ОЧИСТКА РЕСУРСОВ ПРИ ВЫГРУЗКЕ
        StopAllCoroutines();
    }
}