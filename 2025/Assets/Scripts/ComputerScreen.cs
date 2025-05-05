using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComputerScreen : MonoBehaviour
{
    [SerializeField] private Sprite glitchedScreen;
    [SerializeField] private GameObject emailScreen, backgroundScreen;
    private GameObject lastOpenedScreen = null;
    private TextMeshProUGUI screenText, mediaProcessedText, emailText;
    private Image background, foreground;
    private Slider performanceSlider;
    private JobScene jobScene;

    //Emails -------------------------------------//
    [SerializeField] private Transform emailSpawnZone;
    [SerializeField] private Button emailButtonPrefab;
    private List<JobScene.Entry> seenEmails = new();

    //Application Buttons -------------------------------------//
    private Button workButton, emailButton, reviewButton, hackButton;
    private TextMeshProUGUI workButtonText;

    private Color SELECTEDCOLOR = new(0.7843137f, 0.7843137f, 0.7843137f, 1f); 
    private Color NORMALCOLOR = new(1f, 1f, 1f, 1f); 
    

    public void Initalize()
    {
        jobScene = FindFirstObjectByType<JobScene>();
        SetUpButtons();
        SetUpImages();
        SetUpText();
        ShowEMailScreen();
        ComputerStartUp();
    }

    private void SetUpButtons()
    {
        workButton = FindObject<Button>("WorkButton");
        workButton.onClick.AddListener(WorkButtonStart);      
        workButtonText = workButton.GetComponentInChildren<TextMeshProUGUI>();
        if (workButtonText == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on startWorkButton.");
            return;
        }    
        workButtonText.text = "Clock In";

        emailButton = FindObject<Button>("EmailButton");
        emailButton.onClick.AddListener(() =>
        {
            EventManager.PlaySound?.Invoke("switch1", true);
            if(emailScreen.activeSelf == true)
                HideMenus();
            else
                ShowEMailScreen();
        });

        reviewButton = FindObject<Button>("ReviewButton");
        ShowHideButton(reviewButton, false);
        hackButton = FindObject<Button>("HackButton");
        ShowHideButton(hackButton, false);

        performanceSlider = FindObject<Slider>("PerformanceScale");
        performanceSlider.gameObject.SetActive(false);
    }

    private void SetUpImages()
    {
        background = FindObject<Image>("Background");
        foreground = FindObject<Image>("Foreground");
    }

    private void SetUpText()
    {
        screenText = FindObject<TextMeshProUGUI>("ComputerScreenText");
        mediaProcessedText = FindObject<TextMeshProUGUI>("MediaProcessedText");
        emailText = FindObject<TextMeshProUGUI>("EmailBodyText");
    }

    private T FindObject<T>(string name) where T : Component
    {
        return FindComponentByName<T>(name);
    }

    private T FindComponentByName<T>(string name) where T : Component
    {
        T[] components = GetComponentsInChildren<T>(true); // Search all children, even inactive ones
        
        foreach (T component in components)
        {
            if (component.gameObject.name == name)
                return component;
        }

        Debug.LogWarning($"Component '{name}' not found!");
        return null;
    }


    public void WorkButtonStart()
    {
        workButton.interactable = false;
        EventManager.PlaySound?.Invoke("switch1", true);
        jobScene.StartCoroutine(jobScene.BeginWorkDay());
        workButtonText.text = "Clock Out";
        workButton.onClick.RemoveAllListeners();
        workButton.onClick.AddListener(WorkButtonEnd);
    }

    public void WorkButtonEnd()
    {
        workButton.interactable = false;
        EventManager.PlaySound?.Invoke("switch1", true);
        jobScene.gameManager.gameData.money += jobScene.DayProfit;
        jobScene.StartCoroutine(jobScene.NextScene());
        workButton.onClick.RemoveAllListeners();    
    }

    public void EndDaySetUp()
    {
        workButton.interactable = true;
        performanceSlider.gameObject.SetActive(true);
        HideMenus();
    }

    private void ComputerStartUp()
    {
        // For some kind of animation for the computer
        screenText.text = "";
    }

    private void ComputerShutDown()
    {
        // For after workButton is clicked on "Clock Out".
        jobScene.StartCoroutine(jobScene.NextScene());
    }

    private void HideMenus()
    {
        backgroundScreen.SetActive(true);
        emailScreen.SetActive(false);
        lastOpenedScreen = null;
    }

    private void ShowEMailScreen()
    {
        HideMenus();
        emailScreen.SetActive(true);
        backgroundScreen.SetActive(false);
        lastOpenedScreen = emailScreen; 
    }

    public void SetEmailText(string text)
    {
        emailText.text = text;
    }

    public void SetScreenText(string text)
    {
        screenText.text = text;
    }
    public void SetProcessedText(string text)
    {
        mediaProcessedText.text = text;
    }

    public void SetButtonSelected(Button targetButton, bool selected)
    {
        var colors = targetButton.colors;
        colors.normalColor = selected ? SELECTEDCOLOR: NORMALCOLOR;
        targetButton.colors = colors;
    }

    public void SetPerformanceSliderValue(float value)
    {
        performanceSlider.value = value;
    }

    public void ShowHideButton(Button button, bool show)
    {
        button.gameObject.SetActive(show);
    }

    public void ShowHideImage(Image image, bool show)
    {
        image.gameObject.SetActive(show);
    }

    internal void CreateEmails(List<JobScene.Entry> emails)
    {
        seenEmails = emails;
        foreach (JobScene.Entry message in seenEmails)
        {
            Button spawnedEmail = Instantiate(emailButtonPrefab, emailSpawnZone);
            TextMeshProUGUI label = spawnedEmail.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = message.sender + "\n" + message.title;

            spawnedEmail.onClick.AddListener(() => 
            {
                SetEmailText(message.title + "\nFrom: " + message.sender + "\n\n" + message.email);
                EventManager.PlaySound?.Invoke("switch1", true);
            });

            SetEmailText(message.title + "\nFrom: " + message.sender + "\n\n" + message.email);
        }
    }

    internal void EventTrigger(int day, bool startEnd)
    {
        if(day == 3)
        {
            ShowHideImage(foreground, startEnd);
            if(lastOpenedScreen != null)
                lastOpenedScreen.SetActive(!startEnd);
        }
    }
}
