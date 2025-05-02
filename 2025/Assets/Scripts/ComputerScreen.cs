using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComputerScreen : MonoBehaviour
{
    private JobScene jobScene;
    private Sprite computerImage;
    [SerializeField] private GameObject emailScreen;
    [SerializeField] private TextMeshProUGUI emailText;
    [SerializeField] private Button workButton, emailButton;
    private List<Button> emailButtons;
    private List<Button> emails;
    

    private void Awake()
    {
        jobScene = FindFirstObjectByType<JobScene>();

        SetUpButtons();
        EmailSetUp();
        ComputerStartUp();
    }

    private void SetUpButtons()
    {
        TextMeshProUGUI buttonText = workButton.GetComponentInChildren<TextMeshProUGUI>();

        if (buttonText != null)
            buttonText.text = "Clock In";
        else
            Debug.LogError("TextMeshProUGUI component not found on startWorkButton.");

        workButton.onClick.AddListener(() =>
        {
            workButton.gameObject.SetActive(false);
            EventManager.PlaySound?.Invoke("switch1");
            jobScene.StartCoroutine(jobScene.BeginWorkDay());

            if (buttonText != null)
                buttonText.text = "Clock Out";
            else
                Debug.LogError("TextMeshProUGUI component not found on startWorkButton.");
        });

        emailButton.onClick.AddListener(() =>
        {
            EventManager.PlaySound?.Invoke("switch1");
            if(emailScreen.activeSelf == true)
                HideMenus();
            else
                EMail();
        });

    }

    private void ComputerStartUp()
    {

    }

    private void ComputerShutDown()
    {

    }

    private void EMail()
    {
        HideMenus();
        emailScreen.SetActive(true);
    }

    private void EmailSetUp()
    {

    }

    public void SetEmailText(string text)
    {
        Debug.Log("Is this reached?");
        //emailText.text = text;
    }

    private void HackMenu()
    {

    }

    private void PerformanceMenu()
    {

    }
    
    private void Background()
    {

    }

    private void HideMenus()
    {
        emailScreen.SetActive(false);
    }

    private void ShowHideButton(Button button, bool show)
    {
        button.gameObject.SetActive(show);
    }


    private void CreateEmail()
    {

    }

    private void ShowEmail()
    {

    }
}
