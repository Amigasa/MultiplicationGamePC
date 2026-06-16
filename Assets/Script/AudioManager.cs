using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Источники звука")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource uiSource;

    [Header("Фоновая музыка")]
    public AudioClip mainMenuMusic;
    public AudioClip galleryMusic;
    public AudioClip tutorialMusic;
    public AudioClip battleMapMusic;
    public AudioClip coloringMapMusic;
    public AudioClip battleGameMusic;
    public AudioClip coloringGameMusic;
    public AudioClip victoryMusic;
    public AudioClip defeatMusic;

    [Header("Звуки UI")]
    public AudioClip buttonClick;
    public AudioClip buttonHover;
    public AudioClip panelOpen;
    public AudioClip panelClose;

    [Header("Звуки игры - Битва")]
    public AudioClip battleCorrectAnswer;
    public AudioClip battleWrongAnswer;
    public AudioClip playerAttack;
    public AudioClip enemyAttack;
    public AudioClip playerDamage;
    public AudioClip enemyDamage;
    public AudioClip starAppear;
    public AudioClip levelComplete;

    [Header("Звуки игры - Разукрашка")]
    public AudioClip coloringCorrectAnswer;
    public AudioClip coloringWrongAnswer;
    public AudioClip colorAppear;
    public AudioClip drawingComplete;

    [Header("Настройки")]
    public float musicVolume = 0.7f;
    public float sfxVolume = 1.0f;
    public float uiVolume = 1.0f;

    private Dictionary<string, AudioClip> soundDictionary = new Dictionary<string, AudioClip>();
    private string currentMusic = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Загружаем настройки громкости
        LoadAudioSettings();

        // Автоматически включаем музыку для текущей сцены
        PlayMusicForScene(SceneManager.GetActiveScene().name);

        // Следим за сменой сцен
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void InitializeSoundDictionary()
    {
        // Музыка
        soundDictionary["MainMenuMusic"] = mainMenuMusic;
        soundDictionary["GalleryMusic"] = galleryMusic;
        soundDictionary["TutorialMusic"] = tutorialMusic;
        soundDictionary["BattleMapMusic"] = battleMapMusic;
        soundDictionary["ColoringMapMusic"] = coloringMapMusic;
        soundDictionary["BattleGameMusic"] = battleGameMusic;
        soundDictionary["ColoringGameMusic"] = coloringGameMusic;
        soundDictionary["VictoryMusic"] = victoryMusic;
        soundDictionary["DefeatMusic"] = defeatMusic;

        // UI звуки
        soundDictionary["ButtonClick"] = buttonClick;
        soundDictionary["ButtonHover"] = buttonHover;
        soundDictionary["PanelOpen"] = panelOpen;
        soundDictionary["PanelClose"] = panelClose;

        // Битва
        soundDictionary["BattleCorrectAnswer"] = battleCorrectAnswer;
        soundDictionary["BattleWrongAnswer"] = battleWrongAnswer;
        soundDictionary["PlayerAttack"] = playerAttack;
        soundDictionary["EnemyAttack"] = enemyAttack;
        soundDictionary["PlayerDamage"] = playerDamage;
        soundDictionary["EnemyDamage"] = enemyDamage;
        soundDictionary["StarAppear"] = starAppear;
        soundDictionary["LevelComplete"] = levelComplete;

        // Разукрашка
        soundDictionary["ColoringCorrectAnswer"] = coloringCorrectAnswer;
        soundDictionary["ColoringWrongAnswer"] = coloringWrongAnswer;
        soundDictionary["ColorAppear"] = colorAppear;
        soundDictionary["DrawingComplete"] = drawingComplete;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"AudioManager: Смена сцены на {scene.name}");
        PlayMusicForScene(scene.name);
    }

    public void PlayMusicForScene(string sceneName)
    {
        string requiredMusic = GetMusicForScene(sceneName);

        // Если уже играет нужная музыка - не переключаем
        if (currentMusic == requiredMusic)
        {
            Debug.Log($"AudioManager: Музыка уже играет ({currentMusic})");
            return;
        }

        Debug.Log($"AudioManager: Переключаю музыку {currentMusic} -> {requiredMusic}");
        currentMusic = requiredMusic;

        switch (sceneName)
        {
            case "MainMenu":
                PlayMainMenuMusic();
                break;
            case "Gallery":
                PlayGalleryMusic();
                break;
            case "Tutorial":
                PlayTutorialMusic();
                break;
            case "Man":
                PlayBattleMapMusic();
                break;
            case "Woman":
                PlayColoringMapMusic();
                break;
            case "BattleLevel1":
            case "BattleLevel2":
            case "BattleLevel3":
            case "BattleLevel4":
            case "BattleLevel5":
            case "BattleLevel6":
            case "BattleLevel7":
            case "BattleLevel8":
                PlayBattleGameMusic();
                break;
            case "Coloring_Level_1":
            case "Coloring_Level_2":
            case "Coloring_Level_3":
            case "Coloring_Level_4":
            case "Coloring_Level_5":
            case "Coloring_Level_6":
            case "Coloring_Level_7":
            case "Coloring_Level_8":
                PlayColoringGameMusic();
                break;
            default:
                // Для любых других сцен используем музыку главного меню
                PlayMainMenuMusic();
                break;
        }
    }

    string GetMusicForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "MainMenu": return "MainMenu";
            case "Gallery": return "Gallery";
            case "Tutorial": return "Tutorial";
            case "Man": return "BattleMap";
            case "Woman": return "ColoringMap";
            case "BattleLevel1":
            case "BattleLevel2":
            case "BattleLevel3":
            case "BattleLevel4":
            case "BattleLevel5":
            case "BattleLevel6":
            case "BattleLevel7":
            case "BattleLevel8":
                return "BattleGame";
            case "Coloring_Level_1":
            case "Coloring_Level_2":
            case "Coloring_Level_3":
            case "Coloring_Level_4":
            case "Coloring_Level_5":
            case "Coloring_Level_6":
            case "Coloring_Level_7":
            case "Coloring_Level_8":
                return "ColoringGame";
            default: return "MainMenu";
        }
    }

    // === УПРАВЛЕНИЕ МУЗЫКОЙ ===
    public void PlayMainMenuMusic()
    {
        if (mainMenuMusic != null)
        {
            musicSource.clip = mainMenuMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayGalleryMusic()
    {
        if (galleryMusic != null)
        {
            musicSource.clip = galleryMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayTutorialMusic()
    {
        if (tutorialMusic != null)
        {
            musicSource.clip = tutorialMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayBattleMapMusic()
    {
        if (battleMapMusic != null)
        {
            musicSource.clip = battleMapMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayColoringMapMusic()
    {
        if (coloringMapMusic != null)
        {
            musicSource.clip = coloringMapMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayBattleGameMusic()
    {
        if (battleGameMusic != null)
        {
            musicSource.clip = battleGameMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayColoringGameMusic()
    {
        if (coloringGameMusic != null)
        {
            musicSource.clip = coloringGameMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayVictoryMusic()
    {
        if (victoryMusic != null)
        {
            musicSource.clip = victoryMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = false;
            musicSource.Play();
        }
    }

    public void PlayDefeatMusic()
    {
        if (defeatMusic != null)
        {
            musicSource.clip = defeatMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = false;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // === ЗВУКИ SFX ===
    public void PlaySFX(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName) && soundDictionary[soundName] != null)
        {
            sfxSource.PlayOneShot(soundDictionary[soundName], sfxVolume);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    // === ЗВУКИ UI ===
    public void PlayUI(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName) && soundDictionary[soundName] != null)
        {
            uiSource.PlayOneShot(soundDictionary[soundName], uiVolume);
        }
    }

    public void PlayButtonClick()
    {
        PlayUI("ButtonClick");
    }

    public void PlayButtonHover()
    {
        PlayUI("ButtonHover");
    }

    // === НАСТРОЙКИ ГРОМКОСТИ ===
    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        musicSource.volume = volume;
        SaveAudioSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        SaveAudioSettings();
    }

    public void SetUIVolume(float volume)
    {
        uiVolume = volume;
        SaveAudioSettings();
    }

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("UIVolume", uiVolume);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        uiVolume = PlayerPrefs.GetFloat("UIVolume", 1.0f);

        musicSource.volume = musicVolume;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}