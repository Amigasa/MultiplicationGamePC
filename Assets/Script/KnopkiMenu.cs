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
    void Update()
    {

    }
    public void Ex()//Кнопка назад для выхода в меню
    {
        RejimiPanel.SetActive(false);
        SavePanel.SetActive(true);
    }
    public void OnSettingsButtonClick()
    {
        MenuPanel.SetActive(false);
        SettingPanel.SetActive(true);
    }
    public void Exxxx()//Кнопка назад для выхода в меню
    {
        SettingPanel.SetActive(false);
        MenuPanel.SetActive(true);
    }
    public void Exit()//Кнопка выхода в главном меню
    {
        Application.Quit();//Полностью выход из игры
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
