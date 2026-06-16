using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class KnopkiMenu : MonoBehaviour
{
    [SerializeField] private GameObject SavePanel;
    [SerializeField] private GameObject MenuPanel;
    [SerializeField] private GameObject RejimiPanel;
    [SerializeField] private GameObject SettingPanel;

    void Update()
    {
        // ===== ПК УПРАВЛЕНИЕ В МЕНЮ =====
        // Escape - выход
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Если открыта панель настроек - закрыть
            if (SettingPanel.activeSelf)
            {
                Exxxx();
            }
            // Если открыта панель режимов - вернуться в меню
            else if (RejimiPanel.activeSelf)
            {
                Ex();
            }
            // Если открыта панель сохранений - вернуться в меню
            else if (SavePanel.activeSelf)
            {
                // Найти SimpleSlotManager и вызвать BackToMainMenu
                SimpleSlotManager slotManager = FindObjectOfType<SimpleSlotManager>();
                if (slotManager != null)
                    slotManager.BackToMainMenu();
            }
        }
    }

    public void ShowGallery()
    {
        SceneManager.LoadScene(4);
    }

    public void StartTutorial()
    {
        SceneManager.LoadScene(14);
    }

    public void BackFromGallery()
    {
        SceneManager.LoadScene(0);
    }

    public void Ex()
    {
        RejimiPanel.SetActive(false);
        SavePanel.SetActive(true);
    }

    public void OnSettingsButtonClick()
    {
        MenuPanel.SetActive(false);
        SettingPanel.SetActive(true);
    }

    public void Exxxx()
    {
        SettingPanel.SetActive(false);
        MenuPanel.SetActive(true);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Man()
    {
        SceneManager.LoadScene(1);
    }

    public void Woman()
    {
        SceneManager.LoadScene(2);
    }
}