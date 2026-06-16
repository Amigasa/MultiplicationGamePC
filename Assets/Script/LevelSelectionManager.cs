using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectionManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelInfo
    {
        public string levelId;
        public int sceneBuildIndex;
        public Button levelButton;
        public GameObject[] stars;
        public TMP_Text levelText;
        public Image lockIcon;
    }

    [Header("Level Settings")]
    public LevelInfo[] levels;

    void Start()
    {
        RefreshAllLevels();
    }

    public void RefreshAllLevels()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            UpdateLevelDisplay(i);
        }
    }

    void Update()
    {
        // ===== ПК УПРАВЛЕНИЕ =====
        // Escape - выход в меню
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BackToMenu();
        }
    }

    void UpdateLevelDisplay(int levelIndex)
    {
        LevelInfo level = levels[levelIndex];
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);

        int stars = PlayerPrefs.GetInt($"Slot_{currentSlot}_{level.levelId}_stars", 0);
        bool completed = PlayerPrefs.GetInt($"Slot_{currentSlot}_{level.levelId}_completed", 0) == 1;
        bool unlocked = IsLevelUnlocked(levelIndex);

        // ОБНОВЛЯЕМ ЗВЕЗДЫ
        for (int i = 0; i < level.stars.Length; i++)
        {
            if (level.stars[i] != null)
                level.stars[i].SetActive(i < stars);
        }

        // НОВАЯ СИСТЕМА ОТОБРАЖЕНИЯ
        if (level.levelButton != null)
        {
            Image buttonImage = level.levelButton.GetComponent<Image>();

            if (!unlocked)
            {
                // УРОВЕНЬ ЗАКРЫТ - активен только замочек
                level.levelButton.interactable = false;


                if (level.levelText != null)
                    level.levelText.gameObject.SetActive(false);
                if (level.lockIcon != null)
                    level.lockIcon.gameObject.SetActive(true);
            }
            else if (completed)
            {
                // УРОВЕНЬ ПРОЙДЕН - активен текст, белая кнопка, нет замка
                level.levelButton.interactable = true;



                if (level.levelText != null)
                    level.levelText.gameObject.SetActive(true);
                if (level.lockIcon != null)
                    level.lockIcon.gameObject.SetActive(false);
            }
            else
            {
                // УРОВЕНЬ ДОСТУПЕН НО НЕ ПРОЙДЕН - серая кнопка, активен текст
                level.levelButton.interactable = true;


                if (level.levelText != null)
                    level.levelText.gameObject.SetActive(true);
                if (level.lockIcon != null)
                    level.lockIcon.gameObject.SetActive(false);

                if (level.levelText != null)
                    level.levelText.text = (levelIndex + 1).ToString();
            }
        }
    }

    bool IsLevelUnlocked(int levelIndex)
    {
        if (levelIndex == 0) return true;

        int currentSlot = SimpleSlotManager.GetCurrentSlot();
        string previousLevelId = levels[levelIndex - 1].levelId;
        return PlayerPrefs.GetInt($"Slot_{currentSlot}_{previousLevelId}_completed", 0) == 1;
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length) return;

        // Проверяем доступен ли уровень
        if (!IsLevelUnlocked(levelIndex))
        {
            Debug.Log("Уровень еще не доступен!");
            return;
        }

        string levelId = levels[levelIndex].levelId;
        int sceneIndex = levels[levelIndex].sceneBuildIndex;
        PlayerPrefs.SetString("CurrentLevel", levelId);
        PlayerPrefs.Save();

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
