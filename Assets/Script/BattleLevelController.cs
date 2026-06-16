using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

[System.Serializable]
public class LevelProgress
{
    public int starsEarned;
    public bool isCompleted;
}

public class BattleLevelController : MonoBehaviour
{
    [Header("Player & Enemy")]
    public Image playerImage;
    public Image enemyImage;
    public int playerHealth = 3;
    public int enemyHealth = 3;

    [Header("UI Elements")]
    public TMP_Text questionText;
    public Button[] answerButtons;
    public GameObject[] playerHearts;
    public GameObject[] enemyHearts;
    public GameObject victoryPanel;
    public GameObject defeatPanel;

    [Header("Stars Display")]
    public GameObject[] victoryStars;
    public TMP_Text victoryText;

    [Header("Timer UI - Slider")]
    public TMP_Text timerText;
    public Slider timerSlider;
    public Image timerFillImage;

    [Header("Bonus Effects")]
    public TMP_Text bonusText;

    [Header("Game Settings")]
    public string levelId = "Battle_Level_1";

    [Header("Progressive Difficulty")]
    public ProgressiveLevelManager difficultyManager;

    private int currentQuestion = 0;
    private int correctAnswer;
    private bool isGameActive = true;
    private int starsEarned = 0;
    private SmartQuestionGenerator questionGenerator;
    private AdaptiveDifficulty adaptiveDifficulty;
    private float questionStartTime;
    private int correctCombo = 0;
    private float timeRemaining;
    private bool isTimerRunning = false;
    private int totalQuestions = 5;

    void Start()
    {
        levelId = GetCurrentLevelId();
        questionGenerator = new SmartQuestionGenerator();
        adaptiveDifficulty = new AdaptiveDifficulty();

        if (difficultyManager == null)
            difficultyManager = FindObjectOfType<ProgressiveLevelManager>();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBattleGameMusic();
        }

        GenerateQuestion();
        UpdateHearts();
        Debug.Log($"Загружен уровень: {levelId}");
    }

    void Update()
    {
        if (isGameActive && isTimerRunning)
        {
            UpdateTimer();
        }

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

    void UpdateTimer()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            if (timerText != null)
                timerText.text = Mathf.CeilToInt(timeRemaining).ToString();

            if (timerSlider != null)
            {
                float totalTime = GetTimeLimitForCurrentLevel();
                float fillAmount = timeRemaining / totalTime;
                timerSlider.value = fillAmount;

                if (timerFillImage != null)
                {
                    if (fillAmount < 0.3f)
                        timerFillImage.color = Color.red;
                    else if (fillAmount < 0.6f)
                        timerFillImage.color = Color.yellow;
                    else
                        timerFillImage.color = Color.green;
                }
            }
        }
        else
        {
            TimeOut();
        }
    }

    void TimeOut()
    {
        if (!isGameActive) return;

        isTimerRunning = false;
        correctCombo = 0;
        playerHealth--;

        string question = questionText.text;
        int num1 = int.Parse(question.Split('×')[0].Trim());
        int num2 = int.Parse(question.Split('×')[1].Split('=')[0].Trim());
        adaptiveDifficulty.RecordMistake(num1, num2);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("BattleWrongAnswer");
            AudioManager.Instance.PlaySFX("PlayerDamage");
        }

        UpdateHearts();
        currentQuestion++;
        ShowBonusEffect("Время вышло! -1 здоровье");

        if (playerHealth <= 0)
        {
            Defeat();
        }
        else if (currentQuestion < totalQuestions)
        {
            GenerateQuestion();
        }
        else
        {
            if (playerHealth > enemyHealth)
                Victory();
            else if (enemyHealth > playerHealth)
                Defeat();
            else
                Draw();
        }
    }

    float GetTimeLimitForCurrentLevel()
    {
        DifficultySettings difficulty = difficultyManager.GetDifficultyForLevel(levelId);
        return difficulty.timeBonusThreshold * 2f;
    }

    string GetCurrentLevelId()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Contains("1")) return "Battle_Level_1";
        else if (sceneName.Contains("2")) return "Battle_Level_2";
        else if (sceneName.Contains("3")) return "Battle_Level_3";
        else if (sceneName.Contains("4")) return "Battle_Level_4";
        else if (sceneName.Contains("5")) return "Battle_Level_5";
        else if (sceneName.Contains("6")) return "Battle_Level_6";
        else if (sceneName.Contains("7")) return "Battle_Level_7";
        else if (sceneName.Contains("8")) return "Battle_Level_8";
        else return "Battle_Level_1";
    }

    void GenerateQuestion()
    {
        if (!isGameActive) return;

        DifficultySettings difficulty = difficultyManager.GetDifficultyForLevel(levelId);
        totalQuestions = difficulty.totalQuestions;
        int[] weakNumbers = adaptiveDifficulty.GetWeakNumbers();

        QuestionData question = questionGenerator.GenerateQuestion(difficulty, weakNumbers);

        questionText.text = $"{question.number1} × {question.number2} = ?";
        correctAnswer = question.correctAnswer;
        questionStartTime = Time.time;

        timeRemaining = GetTimeLimitForCurrentLevel();
        isTimerRunning = true;

        if (timerSlider != null)
            timerSlider.value = 1f;
        if (timerFillImage != null)
            timerFillImage.color = Color.green;
        if (timerText != null)
            timerText.color = Color.white;

        int[] answers = new int[4];
        int correctIndex = Random.Range(0, 4);
        answers[correctIndex] = correctAnswer;

        int wrongIndex = 0;
        for (int i = 0; i < 4; i++)
        {
            if (i != correctIndex)
            {
                answers[i] = question.wrongAnswers[wrongIndex];
                wrongIndex++;
            }

            answerButtons[i].GetComponentInChildren<TMP_Text>().text = answers[i].ToString();

            int index = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index, answers[index]));
        }
    }

    public void OnAnswerSelected(int buttonIndex, int selectedAnswer)
    {
        if (!isGameActive || !isTimerRunning) return;

        isTimerRunning = false;

        DifficultySettings difficulty = difficultyManager.GetDifficultyForLevel(levelId);
        float reactionTime = Time.time - questionStartTime;
        bool isFast = reactionTime < difficulty.timeBonusThreshold;

        if (selectedAnswer == correctAnswer)
        {
            correctCombo++;
            enemyHealth--;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("BattleCorrectAnswer");
                AudioManager.Instance.PlaySFX("PlayerAttack");
                AudioManager.Instance.PlaySFX("EnemyDamage");
                if (isFast)
                    AudioManager.Instance.PlaySFX("StarAppear");
            }

            if (isFast)
                ShowBonusEffect($"Быстро! ({reactionTime:F1}с)");

            StartCoroutine(AttackEnemy());
        }
        else
        {
            correctCombo = 0;
            playerHealth--;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("BattleWrongAnswer");
                AudioManager.Instance.PlaySFX("EnemyAttack");
                AudioManager.Instance.PlaySFX("PlayerDamage");
            }

            string question = questionText.text;
            int num1 = int.Parse(question.Split('×')[0].Trim());
            int num2 = int.Parse(question.Split('×')[1].Split('=')[0].Trim());
            adaptiveDifficulty.RecordMistake(num1, num2);

            StartCoroutine(AttackPlayer());
        }

        UpdateHearts();
        currentQuestion++;

        if (enemyHealth <= 0)
        {
            Victory();
            return;
        }
        else if (playerHealth <= 0)
        {
            Defeat();
            return;
        }

        if (currentQuestion < totalQuestions)
        {
            GenerateQuestion();
        }
        else
        {
            if (playerHealth > enemyHealth)
                Victory();
            else if (enemyHealth > playerHealth)
                Defeat();
            else
                Draw();
        }
    }

    IEnumerator AttackEnemy()
    {
        Vector2 originalPos = playerImage.rectTransform.anchoredPosition;
        playerImage.rectTransform.anchoredPosition = originalPos + new Vector2(50f, 0);
        yield return new WaitForSeconds(0.3f);
        enemyImage.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        playerImage.rectTransform.anchoredPosition = originalPos;
        enemyImage.color = Color.white;
    }

    IEnumerator AttackPlayer()
    {
        Vector2 originalPos = enemyImage.rectTransform.anchoredPosition;
        enemyImage.rectTransform.anchoredPosition = originalPos + new Vector2(-50f, 0);
        yield return new WaitForSeconds(0.3f);
        playerImage.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        enemyImage.rectTransform.anchoredPosition = originalPos;
        playerImage.color = Color.white;
    }

    void UpdateHearts()
    {
        for (int i = 0; i < playerHearts.Length; i++)
            playerHearts[i].SetActive(i < playerHealth);
        for (int i = 0; i < enemyHearts.Length; i++)
            enemyHearts[i].SetActive(i < enemyHealth);
    }

    IEnumerator AnimateStars()
    {
        for (int i = 0; i < victoryStars.Length; i++)
        {
            if (i < starsEarned)
            {
                victoryStars[i].SetActive(true);
                victoryStars[i].transform.localScale = Vector3.zero;
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX("StarAppear");

                float duration = 0.5f;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    victoryStars[i].transform.localScale = Vector3.one * (elapsed / duration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                victoryStars[i].transform.localScale = Vector3.one;
            }
            yield return new WaitForSeconds(0.2f);
        }
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("LevelComplete");
    }

    void Victory()
    {
        isGameActive = false;
        isTimerRunning = false;
        starsEarned = CalculateStars();
        SaveLevelProgress(levelId, starsEarned);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayVictoryMusic();

        StartCoroutine(AnimateStars());

        if (victoryText != null)
            victoryText.text = $"Победа!\nЗвезд: {starsEarned}/3";

        victoryPanel.SetActive(true);
    }

    void SaveLevelProgress(string levelId, int stars)
    {
        int currentSlot = SimpleSlotManager.GetCurrentSlot();
        PlayerPrefs.SetInt($"Slot_{currentSlot}_{levelId}_stars", stars);
        PlayerPrefs.SetInt($"Slot_{currentSlot}_{levelId}_completed", 1);
        PlayerPrefs.Save();
    }

    int CalculateStars()
    {
        return playerHealth;
    }

    void Defeat()
    {
        isGameActive = false;
        isTimerRunning = false;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDefeatMusic();
        defeatPanel.SetActive(true);
    }

    void Draw()
    {
        isGameActive = false;
        isTimerRunning = false;
        starsEarned = 1;
        SaveLevelProgress(levelId, starsEarned);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDefeatMusic();

        StartCoroutine(AnimateStars());

        if (victoryText != null)
            victoryText.text = $"Ничья!\nЗвезд: {starsEarned}/3";

        victoryPanel.SetActive(true);
    }

    void ShowBonusEffect(string message)
    {
        Debug.Log(message);
        if (bonusText != null)
        {
            bonusText.text = message;
            bonusText.gameObject.SetActive(true);
            StartCoroutine(HideBonusText());
        }
    }

    IEnumerator HideBonusText()
    {
        yield return new WaitForSeconds(2f);
        if (bonusText != null)
            bonusText.gameObject.SetActive(false);
    }

    public void RestartLevel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
        SceneManager.LoadScene(1);
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
}