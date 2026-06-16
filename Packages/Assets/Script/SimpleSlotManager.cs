using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SimpleSlotManager : MonoBehaviour
{
    [System.Serializable]
    public class SaveSlotUI
    {
        public Button slotButton;
        public TMP_Text slotNameText;
        public TMP_Text progressText;
        public GameObject deleteButton;
    }

    [Header("UI References")]
    public SaveSlotUI[] saveSlots;
    public GameObject mainMenuPanel;
    public GameObject saveSelectionPanel;
    public GameObject RejimiPanel;

    void Start()
    {
        UpdateAllSlotsDisplay();
    }

    // Показывает меню выбора слотов
    public void ShowSaveSelection()
    {
        mainMenuPanel.SetActive(false);
        saveSelectionPanel.SetActive(true);
        UpdateAllSlotsDisplay();
    }

    // Возврат в главное меню
    public void BackToMainMenu()
    {
        saveSelectionPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Выбор слота сохранения
    public void SelectSlot(int slotIndex)
    {
        bool slotExists = PlayerPrefs.GetInt($"Slot_{slotIndex}_Exists", 0) == 1;

        if (!slotExists)
        {
            // Создаем новое сохранение
            CreateNewSlot(slotIndex);
        }

        PlayerPrefs.SetInt("CurrentSaveSlot", slotIndex);
        PlayerPrefs.Save();

        Debug.Log($"Выбран слот: {slotIndex}");

        // Переходим к выбору режима
        saveSelectionPanel.SetActive(false);
        RejimiPanel.SetActive(true);
    }

    // Удаление слота
    public void DeleteSlot(int slotIndex)
    {
        // Удаляем все данные слота - БИТВА
        for (int i = 1; i <= 17; i++)
        {
            PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Battle_Level_{i}_stars");
            PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Battle_Level_{i}_completed");
        }

        // Удаляем все данные слота - РАСКРАСКА (ДОБАВИТЬ)
        for (int i = 1; i <= 4; i++)
        {
            PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Coloring_Level_{i}_stars");
            PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Coloring_Level_{i}_completed");
        }

        // Удаляем все данные галереи (ДОБАВИТЬ)
        for (int i = 1; i <= 4; i++)
        {
            PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Gallery_Level_{i}");
        }

        PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Exists");
        PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Name");
        PlayerPrefs.Save();

        UpdateAllSlotsDisplay();
        Debug.Log($"Слот {slotIndex} полностью очищен");
    }

    // Создание нового слота
    void CreateNewSlot(int slotIndex)
    {
        PlayerPrefs.SetInt($"Slot_{slotIndex}_Exists", 1);
        PlayerPrefs.SetString($"Slot_{slotIndex}_Name", $"Сохранение {slotIndex + 1}");
        PlayerPrefs.Save();
    }

    // Обновление отображения всех слотов
    void UpdateAllSlotsDisplay()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            UpdateSlotDisplay(i);
        }
    }

    // Обновление отображения конкретного слота
    void UpdateSlotDisplay(int slotIndex)
    {
        bool slotExists = PlayerPrefs.GetInt($"Slot_{slotIndex}_Exists", 0) == 1;
        SaveSlotUI slotUI = saveSlots[slotIndex];

        if (slotExists)
        {
            string slotName = PlayerPrefs.GetString($"Slot_{slotIndex}_Name", $"Сохранение {slotIndex + 1}");
            int completedLevels = CountCompletedLevels(slotIndex);
            int totalStars = CountTotalStars(slotIndex);

            slotUI.slotNameText.text = slotName;
            slotUI.progressText.text = $"Уровней: {completedLevels}/21\nЗвезд: {totalStars}/63"; // 17+4=21 уровней, 51+12=63 звезды
            slotUI.deleteButton.SetActive(true);
        }
        else
        {
            slotUI.slotNameText.text = $"Сохранение {slotIndex + 1}";
            slotUI.progressText.text = "Новое сохранение";
            slotUI.deleteButton.SetActive(false);
        }
    }

    // Подсчет пройденных уровней (ОБНОВИТЬ)
    int CountCompletedLevels(int slotIndex)
    {
        int count = 0;

        // Уровни битвы (17)
        for (int i = 1; i <= 17; i++)
        {
            if (PlayerPrefs.GetInt($"Slot_{slotIndex}_Battle_Level_{i}_completed", 0) == 1)
                count++;
        }

        // Уровни раскраски (4) - ДОБАВИТЬ
        for (int i = 1; i <= 4; i++)
        {
            if (PlayerPrefs.GetInt($"Slot_{slotIndex}_Coloring_Level_{i}_completed", 0) == 1)
                count++;
        }

        return count;
    }

    // Подсчет общего количества звезд (ОБНОВИТЬ)
    int CountTotalStars(int slotIndex)
    {
        int total = 0;

        // Звезды уровней битвы
        for (int i = 1; i <= 17; i++)
        {
            total += PlayerPrefs.GetInt($"Slot_{slotIndex}_Battle_Level_{i}_stars", 0);
        }

        // Звезды уровней раскраски - ДОБАВИТЬ
        for (int i = 1; i <= 4; i++)
        {
            total += PlayerPrefs.GetInt($"Slot_{slotIndex}_Coloring_Level_{i}_stars", 0);
        }

        return total;
    }

    // Получить текущий выбранный слот
    public static int GetCurrentSlot()
    {
        return PlayerPrefs.GetInt("CurrentSaveSlot", 0);
    }
}