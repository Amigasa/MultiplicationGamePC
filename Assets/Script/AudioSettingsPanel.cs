using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSettingsPanel : MonoBehaviour
{
    [Header("Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider uiSlider;

    [Header("Texts")]
    public TMP_Text musicText;
    public TMP_Text sfxText;
    public TMP_Text uiText;

    [Header("Mute Buttons")]
    public Button musicMuteButton;
    public Button sfxMuteButton;
    public Button uiMuteButton;

    private bool isMusicMuted = false;
    private bool isSFXMuted = false;
    private bool isUIMuted = false;
    private float musicVolumeBeforeMute;
    private float sfxVolumeBeforeMute;
    private float uiVolumeBeforeMute;

    void Start()
    {
        InitializeSettings();
        SetupEventListeners();
    }

    void InitializeSettings()
    {
        // Загружаем текущие настройки
        musicSlider.value = AudioManager.Instance.musicVolume;
        sfxSlider.value = AudioManager.Instance.sfxVolume;
        uiSlider.value = AudioManager.Instance.uiVolume;

        UpdateTexts();
        UpdateMuteButtons();
    }

    void SetupEventListeners()
    {
        // Слайдеры
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        uiSlider.onValueChanged.AddListener(OnUIVolumeChanged);

        // Кнопки mute
        if (musicMuteButton != null)
            musicMuteButton.onClick.AddListener(ToggleMusicMute);
        if (sfxMuteButton != null)
            sfxMuteButton.onClick.AddListener(ToggleSFXMute);
        if (uiMuteButton != null)
            uiMuteButton.onClick.AddListener(ToggleUIMute);
    }

    void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
        musicText.text = $"{Mathf.RoundToInt(value * 100)}%";

        // Если включаем звук после mute - снимаем mute
        if (isMusicMuted && value > 0)
        {
            isMusicMuted = false;
            UpdateMuteButtons();
        }
    }

    void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
        sfxText.text = $"{Mathf.RoundToInt(value * 100)}%";

        if (isSFXMuted && value > 0)
        {
            isSFXMuted = false;
            UpdateMuteButtons();
        }
    }

    void OnUIVolumeChanged(float value)
    {
        AudioManager.Instance.SetUIVolume(value);
        uiText.text = $"{Mathf.RoundToInt(value * 100)}%";

        if (isUIMuted && value > 0)
        {
            isUIMuted = false;
            UpdateMuteButtons();
        }
    }

    void ToggleMusicMute()
    {
        if (!isMusicMuted)
        {
            // Mute
            musicVolumeBeforeMute = musicSlider.value;
            musicSlider.value = 0;
            isMusicMuted = true;
        }
        else
        {
            // Unmute
            musicSlider.value = musicVolumeBeforeMute > 0 ? musicVolumeBeforeMute : 0.7f;
            isMusicMuted = false;
        }
        UpdateMuteButtons();
    }

    void ToggleSFXMute()
    {
        if (!isSFXMuted)
        {
            sfxVolumeBeforeMute = sfxSlider.value;
            sfxSlider.value = 0;
            isSFXMuted = true;
        }
        else
        {
            sfxSlider.value = sfxVolumeBeforeMute > 0 ? sfxVolumeBeforeMute : 1.0f;
            isSFXMuted = false;
        }
        UpdateMuteButtons();
    }

    void ToggleUIMute()
    {
        if (!isUIMuted)
        {
            uiVolumeBeforeMute = uiSlider.value;
            uiSlider.value = 0;
            isUIMuted = true;
        }
        else
        {
            uiSlider.value = uiVolumeBeforeMute > 0 ? uiVolumeBeforeMute : 1.0f;
            isUIMuted = false;
        }
        UpdateMuteButtons();
    }

    void UpdateTexts()
    {
        musicText.text = $"{Mathf.RoundToInt(musicSlider.value * 100)}%";
        sfxText.text = $"{Mathf.RoundToInt(sfxSlider.value * 100)}%";
        uiText.text = $"{Mathf.RoundToInt(uiSlider.value * 100)}%";
    }

    void UpdateMuteButtons()
    {
        // Обновляем текст кнопок mute
        if (musicMuteButton != null)
        {
            TextMeshProUGUI muteText = musicMuteButton.GetComponentInChildren<TextMeshProUGUI>();
            if (muteText != null)
                muteText.text = isMusicMuted ? "🔈" : "🔇";
        }

        if (sfxMuteButton != null)
        {
            TextMeshProUGUI muteText = sfxMuteButton.GetComponentInChildren<TextMeshProUGUI>();
            if (muteText != null)
                muteText.text = isSFXMuted ? "🔈" : "🔇";
        }

        if (uiMuteButton != null)
        {
            TextMeshProUGUI muteText = uiMuteButton.GetComponentInChildren<TextMeshProUGUI>();
            if (muteText != null)
                muteText.text = isUIMuted ? "🔈" : "🔇";
        }
    }

    // Методы для открытия/закрытия панели
    public void ShowPanel()
    {
        gameObject.SetActive(true);
        // Звук открытия панели
        AudioManager.Instance.PlayUI("PanelOpen");
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
        // Звук закрытия панели
        AudioManager.Instance.PlayUI("PanelClose");
    }
}