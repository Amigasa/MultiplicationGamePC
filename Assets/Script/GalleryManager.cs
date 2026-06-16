using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GalleryManager : MonoBehaviour
{
    [Header("Элементы галереи")]
    public Button[] galleryButtons;
    public Image[] galleryImage;
    public GameObject[] lockIcons;
    public TMP_Text emptyText;

    [Header("Спрайты рисунков")]
    public Sprite[] drawingSprites;

    void Start()
    {
        UpdateGallery();
        SetupButtonListeners();
    }

    void UpdateGallery()
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);
        bool hasAnyDrawing = false;

        for (int i = 0; i < galleryButtons.Length; i++)
        {
            // Проверяем разблокирован ли рисунок в текущем слоте
            bool isUnlocked = PlayerPrefs.GetInt($"Slot_{currentSlot}_Gallery_Level_{i + 1}", 0) == 1;

            Image buttonImage = galleryButtons[i].GetComponent<Image>();
            Image galleryI = galleryImage[i].GetComponent<Image>();

            if (isUnlocked)
            {
                // Рисунок разблокирован
                buttonImage.sprite = drawingSprites[i];
                galleryI.color = Color.white;
                lockIcons[i].SetActive(false);
                hasAnyDrawing = true;
            }
            else
            {
                // Рисунок заблокирован
                buttonImage.sprite = drawingSprites[i];
                galleryI.color = Color.black;
                lockIcons[i].SetActive(true);
            }
        }

        if (emptyText != null)
        {
            emptyText.gameObject.SetActive(!hasAnyDrawing);
        }
    }

    void SetupButtonListeners()
    {
        for (int i = 0; i < galleryButtons.Length; i++)
        {
            int index = i;
            galleryButtons[i].onClick.AddListener(() => ViewDrawing(index));
        }
    }

    void ViewDrawing(int drawingIndex)
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);
        bool isUnlocked = PlayerPrefs.GetInt($"Slot_{currentSlot}_Gallery_Level_{drawingIndex + 1}", 0) == 1;

        if (isUnlocked)
        {
            Debug.Log($"Смотришь рисунок {drawingIndex + 1}");
            // Можно добавить полноэкранный просмотр
        }
        else
        {
            Debug.Log($"Сначала пройди уровень {drawingIndex + 1} без ошибок!");
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }
}