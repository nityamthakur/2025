using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComputerScreen : MonoBehaviour
{
    [SerializeField] private Sprite glitchedScreen, computerLogo;
    [SerializeField] private GameObject emailScreen, applicationBar, backgroundScreen;
    private GameObject lastOpenedScreen = null;
    private TextMeshProUGUI screenText, mediaProcessedText, emailText;
    private Image background, foreground, resultsBackground, fadingImage, emailNotification;
    private Slider performanceSlider;
    private JobScene jobScene;

    //Emails -------------------------------------//
    [SerializeField] private Transform emailSpawnZone;
    [SerializeField] private Button emailButtonPrefab;
    private int unreadEmailCount = 0;

    //Application Buttons -------------------------------------//
    private Button workButton, emailButton, reviewButton, hackButton;
    private TextMeshProUGUI workButtonText, emailNotificationText;

    private Color SELECTEDCOLOR = new(0.7843137f, 0.7843137f, 0.7843137f, 1f); 
    private Color NORMALCOLOR = new(1f, 1f, 1f, 1f); 
    

    public void Initalize()
    {
        jobScene = FindFirstObjectByType<JobScene>();
        SetUpButtons();
        SetUpImages();
        SetUpText();
        ShowEMailScreen();
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
        fadingImage = FindObject<Image>("FadingImage");
        resultsBackground = FindObject<Image>("ResultsBackground");
        resultsBackground.gameObject.SetActive(false);
        emailNotification = FindObject<Image>("EmailNotification");
        emailNotificationText = emailNotification.transform.Find("Text").GetComponent<TextMeshProUGUI>();
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
        resultsBackground.gameObject.SetActive(true);
        screenText.gameObject.SetActive(true);
        HideMenus();
    }

    public void StartComputer()
    {
        StartCoroutine(ComputerStartUp());
    }

    private IEnumerator ComputerStartUp()
    {
        // For some kind of animation for the computer
        screenText.text = "";
        applicationBar.SetActive(false);
        emailScreen.SetActive(false);
        fadingImage.gameObject.SetActive(true);
        foreground.gameObject.SetActive(true);

        // Screen starts as black
        // Wait for 1 second
        yield return new WaitForSeconds(8f);
        
        // Screen goes to a lighter shader of dark blue showing the screen is on
        fadingImage.color = new Color(0.0f, 0.0f, 0.2f, 1.0f);
        
        // Wait for 1 second
        yield return new WaitForSeconds(2f);
        
        // Fade in the Computer Logo, 0.5 seconds
        foreground.sprite = computerLogo;
        StartCoroutine(FadeImage(fadingImage, 0.5f, false));
        yield return new WaitForSeconds(5.0f);
        
        // Show loading bar, 2 seconds
        // LoadingBarAnimation(2f);
        // Fade out the Computer Logo, 0.5 seconds
        StartCoroutine(FadeImage(fadingImage, 0.5f, true));
        yield return new WaitForSeconds(2f);

        // Show background of computer being on
        foreground.gameObject.SetActive(false);
        fadingImage.gameObject.SetActive(false);
        applicationBar.SetActive(true);
        HideMenus();
        UpdateUnreadEmailsPopUp();
        // Show unread emails to the upper right of the email button. Look up iphone unread emails
    }

    private IEnumerator FadeImage(Image image, float duration, bool fadein)
    {
        if(fadein)
            yield return StartCoroutine(FadeImage(image, 0f, 1f, duration)); // goes to 100% opacity
        else
            yield return StartCoroutine(FadeImage(image, 1f, 0f, duration)); // goes to 0% opacity
    }

    private IEnumerator FadeImage(Image image, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        Color color = image.color;
        color.a = startAlpha;
        image.color = color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            image.color = color;
            yield return null;
        }

        color.a = endAlpha;
        image.color = color;
    }


    private IEnumerator ComputerShutDown()
    {
        // For after workButton is clicked on "Clock Out".
        // Have screen go to computer logo
        foreground.sprite = computerLogo;

        // Show loading bar for shutting down, 2 seconds
        //LoadingBarAnimation(2f);
        
        // Screen goes to a lighter shader of dark blue showing the screen is on
        fadingImage.gameObject.SetActive(true);
        foreground.gameObject.SetActive(false);
        // Screen goes to black

        yield return new WaitForSeconds(1f);

        fadingImage.color = Color.black;

        yield return new WaitForSeconds(1f);
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

    private void UpdateUnreadEmailsPopUp()
    {
        unreadEmailCount = Math.Clamp(unreadEmailCount, 0, int.MaxValue);
        emailNotificationText.text = $"{unreadEmailCount}";
        if(unreadEmailCount == 0)
                emailNotification.gameObject.SetActive(false);
    }

    internal void CreateEmails(List<JobScene.Entry> releasedEmails)
    {
        unreadEmailCount = 0;
        foreach(JobScene.Entry email in releasedEmails)
        {
            Button spawnedEmail = Instantiate(emailButtonPrefab, emailSpawnZone);
            Image emailReadIndicator = spawnedEmail.transform.Find("EmailReadIndicator").GetComponent<Image>();
            TextMeshProUGUI label = spawnedEmail.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = email.sender + "\n" + email.title;

            spawnedEmail.onClick.AddListener(() => 
            {
                EventManager.PlaySound?.Invoke("switch1", true);
                SetEmailText(email.title + "\nFrom: " + email.sender + "\n\n" + email.email);
                if(!email.seen)
                {
                    EmailCountUpdate(-1);
                    email.seen = true;
                    emailReadIndicator.color = Color.white;
                }
            });

            if(!email.seen)
                EmailCountUpdate(+1);
            else
                emailReadIndicator.color = Color.white;
            //SetEmailText(email.title + "\nFrom: " + email.sender + "\n\n" + email.email);
        }
        SetEmailText("");
    }

    private void EmailCountUpdate(int num)
    {
        unreadEmailCount += num;
        UpdateUnreadEmailsPopUp();
    }

    internal void EventTrigger(int day, bool startEnd)
    {
        if(day == 3)
        {
            ShowHideImage(foreground, startEnd);
            foreground.sprite = glitchedScreen;
            if(lastOpenedScreen != null)
                lastOpenedScreen.SetActive(!startEnd);
        }
    }
}
