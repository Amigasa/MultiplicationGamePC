using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Dialogue
{
    public string speakerName;
    [TextArea(3, 10)]
    public string sentence;
    public bool isCharacter1Speaking; // true - первый персонаж, false - второй
}

public class TutorialManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText;
    public Image character1Image;
    public Image character2Image;
    public Button backButton;

    [Header("Dialogue Settings")]
    public Dialogue[] dialogues;
    public float typingSpeed = 0.05f;

    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    [Header("Character Colors")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    void Start()
    {
        // Воспроизводим музыку для обучения
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTutorialMusic();
        }

        backButton.onClick.AddListener(BackToMainMenu);

        // Начинаем первый диалог
        ShowDialogue(currentDialogueIndex);
    }

    void ShowDialogue(int index)
    {
        if (index >= dialogues.Length)
        {
            // Диалоги закончились, возвращаем в меню
            CompleteTutorial();
            return;
        }

        Dialogue currentDialogue = dialogues[index];

        // Устанавливаем имя говорящего
        speakerNameText.text = currentDialogue.speakerName;

        // Подсвечиваем активного персонажа
        if (currentDialogue.isCharacter1Speaking)
        {
            character1Image.color = activeColor;
            character2Image.color = inactiveColor;

            // Звук смены говорящего на персонажа 1
            if (AudioManager.Instance != null && currentDialogueIndex > 0)
            {
                AudioManager.Instance.PlaySFX("PanelOpen");
            }
        }
        else
        {
            character1Image.color = inactiveColor;
            character2Image.color = activeColor;

            // Звук смены говорящего на персонажа 2
            if (AudioManager.Instance != null && currentDialogueIndex > 0)
            {
                AudioManager.Instance.PlaySFX("PanelOpen");
            }
        }

        // Запускаем анимацию печати текста
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeSentence(currentDialogue.sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;

            // Звук печати текста (воспроизводим не для каждого символа, чтобы не было слишком часто)
            if (AudioManager.Instance != null && letter != ' ' && Random.Range(0, 3) == 0)
            {
                AudioManager.Instance.PlaySFX("ButtonClick");
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // Звук завершения печати текста
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("PanelClose");
        }
    }

    void ContinueDialogue()
    {
        // Звук продолжения диалога
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        if (isTyping)
        {
            // Если текст еще печатается, завершить печать
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = dialogues[currentDialogueIndex].sentence;
            isTyping = false;

            // Звук пропуска текста
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("PanelClose");
            }
            return;
        }

        currentDialogueIndex++;
        ShowDialogue(currentDialogueIndex);
    }

    void CompleteTutorial()
    {
        // Сохраняем пройденное обучение
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();

        // Звук завершения обучения
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("LevelComplete");
        }

        BackToMainMenu();
    }

    void BackToMainMenu()
    {
        // Звук кнопки возврата
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
        SceneManager.LoadScene(0); // Замените на имя вашей сцены главного меню
    }

    void Update()
    {
        // Добавляем возможность продолжать по нажатию пробела или клику мыши
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            ContinueDialogue();
        }

        // Кнопка Назад на Android
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BackToMainMenu();
        }
    }

    // Методы для кнопок (если нужно привязать к UI)
    public void OnBackButtonClicked()
    {
        BackToMainMenu();
    }

    public void OnContinueButtonClicked() // Если добавите кнопку продолжения
    {
        ContinueDialogue();
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
}
