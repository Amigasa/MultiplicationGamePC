using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SoundButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [Header("Настройки звуков")]
    public bool playClickSound = true;
    public bool playHoverSound = true;
    public string customClickSound;
    public string customHoverSound;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (playClickSound)
        {
            if (!string.IsNullOrEmpty(customClickSound))
            {
                AudioManager.Instance.PlayUI(customClickSound);
            }
            else
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playHoverSound)
        {
            if (!string.IsNullOrEmpty(customHoverSound))
            {
                AudioManager.Instance.PlayUI(customHoverSound);
            }
            else
            {
                AudioManager.Instance.PlayButtonHover();
            }
        }
    }

    private void OnButtonClick()
    {
        // Резервный метод для кнопок Unity
        if (playClickSound && AudioManager.Instance != null)
        {
            if (!string.IsNullOrEmpty(customClickSound))
            {
                AudioManager.Instance.PlayUI(customClickSound);
            }
            else
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }
    }
}
