using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComputerScreen : MonoBehaviour
{
    [SerializeField] private Transform screenZoomIn;
    [SerializeField] private Sprite glitchedScreen, computerLogo;
    [SerializeField] private GameObject emailScreen, reviewScreen, applicationBar, backgroundScreen;
    private GameObject lastOpenedScreen = null;
    private TextMeshProUGUI screenText, mediaProcessedText, emailText, reviewText, reviewMediaText;
    private Image background, foreground, resultsBackground, fadingImage, emailNotification, reviewNotification, loadingImage;
    private Slider performanceSlider;
    private JobScene jobScene;
    private Sprite[] loadingCircleAnimation;

    //Emails -------------------------------------//
    [SerializeField] private Transform emailSpawnZone;
    [SerializeField] private Button emailButtonPrefab;
    private int unreadEmailCount = 0;

    //Review Menu -------------------------------------//
    [SerializeField] private Transform reviewSpawnZone;
    private List<Media> currentDayMedia = new();
    private int unreadReviewCount = 0, dayMediaIterator = 0;
    private Button mostRecentReviews;

    //Application Buttons -------------------------------------//
    private Button workButton, emailButton, reviewButton, hackButton, nextButton, previousButton;
    private TextMeshProUGUI workButtonText, emailNotificationText, reviewNotificationText;

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
            if (emailScreen.activeSelf == true)
                HideMenus();
            else
                ShowEMailScreen();
        });

        reviewButton = FindObject<Button>("ReviewButton");
        reviewButton.onClick.AddListener(() =>
        {
            EventManager.PlaySound?.Invoke("switch1", true);
            if (reviewScreen.activeSelf == true)
                HideMenus();
            else
                ShowReviewScreen();
        });

        hackButton = FindObject<Button>("HackButton");
        ShowHideButton(hackButton, false);

        nextButton = FindObject<Button>("NextButton");
        nextButton.onClick.AddListener(() =>
        {
            dayMediaIterator = (dayMediaIterator + 1) % currentDayMedia.Count;
            ReviewArticleTextUpdate();
        });
        previousButton = FindObject<Button>("PreviousButton");
        previousButton.onClick.AddListener(() =>
        {
            dayMediaIterator = (dayMediaIterator - 1 + currentDayMedia.Count) % currentDayMedia.Count;
            ReviewArticleTextUpdate();
        });

        performanceSlider = FindObject<Slider>("PerformanceScale");
        performanceSlider.gameObject.SetActive(false);
    }

    private void SetUpImages()
    {
        background = FindObject<Image>("Background");
        foreground = FindObject<Image>("Foreground");
        loadingImage = FindObject<Image>("LoadingCircle");
        fadingImage = FindObject<Image>("FadingImage");
        resultsBackground = FindObject<Image>("ResultsBackground");
        resultsBackground.gameObject.SetActive(false);

        emailNotification = FindObject<Image>("EmailNotification");
        emailNotificationText = emailNotification.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        reviewNotification = FindObject<Image>("ReviewNotification");
        reviewNotificationText = reviewNotification.transform.Find("Text").GetComponent<TextMeshProUGUI>();

        loadingCircleAnimation = Resources.LoadAll<Sprite>("Animations/ComputerLoadingAnimation");
    }

    private void SetUpText()
    {
        screenText = FindObject<TextMeshProUGUI>("ComputerScreenText");
        mediaProcessedText = FindObject<TextMeshProUGUI>("MediaProcessedText");
        emailText = FindObject<TextMeshProUGUI>("EmailBodyText");
        reviewText = FindObject<TextMeshProUGUI>("ReviewBodyText");
        reviewMediaText = FindObject<TextMeshProUGUI>("ReviewMediaText");
        reviewMediaText.gameObject.SetActive(false);
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
        jobScene.StartCoroutine(jobScene.NextScene());
        workButton.onClick.RemoveAllListeners();
    }

    public void EndDaySetUp()
    {
        workButton.interactable = true;
        performanceSlider.gameObject.SetActive(true);
        resultsBackground.gameObject.SetActive(true);
        screenText.gameObject.SetActive(true);
        mostRecentReviews.gameObject.SetActive(true);
        EventManager.StopClockMovement?.Invoke();
        ReviewCountUpdate(+1);
        HideMenus();
        EventManager.ZoomCamera?.Invoke(screenZoomIn, 3.1f, 0.25f);
    }

    public void StartComputer()
    {
        StartCoroutine(ComputerStartUp());
        //StartCoroutine(ComputerInstantStartUp());
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
        yield return new WaitForSeconds(1f);

        // Screen goes to a lighter shader of dark blue showing the screen is on
        fadingImage.color = new Color(0.0f, 0.0f, 0.2f, 1.0f);

        // Wait for 1 second
        yield return new WaitForSeconds(2f);

        // Fade in the Computer Logo, 0.5 seconds
        foreground.sprite = computerLogo;
        StartCoroutine(FadeImage(fadingImage, 0.5f, false));
        StartCoroutine(CycleBackgroundFrames(loadingCircleAnimation, 6f, 0.2f));
        yield return new WaitForSeconds(5.0f);

        // Fade out the Computer Logo, 0.5 seconds
        StartCoroutine(FadeImage(fadingImage, 0.5f, true));
        EventManager.PlaySound?.Invoke("computerChime", true);

        yield return new WaitForSeconds(2f);

        // Show background of computer being on
        loadingImage.gameObject.SetActive(false);
        foreground.gameObject.SetActive(false);
        fadingImage.gameObject.SetActive(false);
        applicationBar.SetActive(true);
        HideMenus();
        UpdateUnreadPopUps();
        // Show unread emails to the upper right of the email button. Look up iphone unread emails
    }

    private IEnumerator ComputerInstantStartUp()
    {
        // For some kind of animation for the computer
        screenText.text = "";
        applicationBar.SetActive(false);
        emailScreen.SetActive(false);
        fadingImage.gameObject.SetActive(true);
        foreground.gameObject.SetActive(true);

        // Show background of computer being on
        foreground.gameObject.SetActive(false);
        fadingImage.gameObject.SetActive(false);
        applicationBar.SetActive(true);
        HideMenus();
        UpdateUnreadPopUps();
        yield return new();
        // Show unread emails to the upper right of the email button. Look up iphone unread emails
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


    private IEnumerator CycleBackgroundFrames(Sprite[] frames, float duration, float frameInterval)
    {
        if (frames == null || frames.Length == 0)
            yield break;

        int index = 0;
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            loadingImage.sprite = frames[index];
            index = (index + 1) % frames.Length;
            yield return new WaitForSeconds(frameInterval);
        }
    }

    private IEnumerator FadeImage(Image image, float duration, bool fadein)
    {
        if (fadein)
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

    private void HideMenus()
    {
        backgroundScreen.SetActive(true);
        emailScreen.SetActive(false);
        reviewScreen.SetActive(false);
        lastOpenedScreen = null;
    }

    private void ShowEMailScreen()
    {
        HideMenus();
        emailScreen.SetActive(true);
        backgroundScreen.SetActive(false);
        lastOpenedScreen = emailScreen;
    }

    private void ShowReviewScreen()
    {
        HideMenus();
        reviewScreen.SetActive(true);
        backgroundScreen.SetActive(false);
        lastOpenedScreen = reviewScreen;
    }

    private void SetObjectText(TextMeshProUGUI objectText, string text)
    {
        objectText.text = text;
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
        colors.normalColor = selected ? SELECTEDCOLOR : NORMALCOLOR;
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

    private void UpdateUnreadPopUps()
    {
        unreadEmailCount = Math.Clamp(unreadEmailCount, 0, int.MaxValue);
        emailNotificationText.text = $"{unreadEmailCount}";
        if (unreadEmailCount == 0)
            emailNotification.gameObject.SetActive(false);

        unreadReviewCount = Math.Clamp(unreadReviewCount, 0, int.MaxValue);
        reviewNotificationText.text = $"{unreadReviewCount}";
        if (unreadReviewCount == 0)
            reviewNotification.gameObject.SetActive(false);
    }

    internal void CreateEmails(List<JobScene.Entry> releasedEmails)
    {
        unreadEmailCount = 0;
        foreach (JobScene.Entry email in releasedEmails)
        {
            Button spawnedEmail = Instantiate(emailButtonPrefab, emailSpawnZone);
            Image emailReadIndicator = spawnedEmail.transform.Find("EmailReadIndicator").GetComponent<Image>();
            TextMeshProUGUI label = spawnedEmail.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = email.sender + "\n" + email.title;

            spawnedEmail.onClick.AddListener(() =>
            {
                EventManager.PlaySound?.Invoke("switch1", true);
                SetObjectText(emailText, email.title + "\nFrom: " + email.sender + "\n\n" + email.email);
                if (!email.seen)
                {
                    EmailCountUpdate(-1);
                    email.seen = true;
                    emailReadIndicator.color = Color.white;
                }
            });

            if (!email.seen)
                EmailCountUpdate(+1);
            else
                emailReadIndicator.color = Color.white;
            //SetEmailText(email.title + "\nFrom: " + email.sender + "\n\n" + email.email);
        }
        SetObjectText(emailText, "");
    }

    private void EmailCountUpdate(int num)
    {
        unreadEmailCount += num;
        UpdateUnreadPopUps();
    }

    internal void CreateReviews(List<Media> dayMedia, int day)
    {
        unreadReviewCount = 0;

        for (int i = 1; i <= day; i++)
        {
            int thisDay = i;
            Button spawnedReview = Instantiate(emailButtonPrefab, reviewSpawnZone);
            Image reviewReadIndicator = spawnedReview.transform.Find("EmailReadIndicator").GetComponent<Image>();
            TextMeshProUGUI label = spawnedReview.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = $"\nDay: {i} Review";

            spawnedReview.onClick.AddListener(() =>
            {
                EventManager.PlaySound?.Invoke("switch1", true);
                dayMediaIterator = 0;

                if (dayMedia.Count > 0)
                {
                    currentDayMedia = dayMedia.Where(media => media.day == thisDay).ToList(); // Filter media by the selected day
                }

                ReviewArticleTextUpdate();
                /*
                if (!email.seen)
                {
                    ReviewCountUpdate(-1);
                    email.seen = true;
                    reviewReadIndicator.color = Color.white;
                }
                */

                ShowHideButton(nextButton, currentDayMedia.Count >= 2);
                ShowHideButton(previousButton, currentDayMedia.Count >= 2);
                reviewMediaText.gameObject.SetActive(true);
            });

            //if (!email.seen)
            //ReviewCountUpdate(+1);
            //else
            //reviewReadIndicator.color = Color.white;

            if (i == day)
            {
                spawnedReview.gameObject.SetActive(false);
                mostRecentReviews = spawnedReview;
            }
        }
        string text = "\nView any mistakes you may have made on past articles. \n\nAny Articles released today wont be ready for view until the end of your work shift.";
        SetObjectText(reviewText, text);
        ShowHideButton(nextButton, currentDayMedia.Count >= 2);
        ShowHideButton(previousButton, currentDayMedia.Count >= 2);
        ReviewCountUpdate(+1);
    }

    private void ReviewArticleTextUpdate()
    {
        if (currentDayMedia.Count > 0)
        {
            reviewMediaText.text = $"Review:\n{dayMediaIterator + 1}/ {currentDayMedia.Count}";

            Media article = currentDayMedia[dayMediaIterator];
            string text = "";
            if (article.noMistakes)
                text += "Well Done! There were no mistakes made on review of this article.";
            else
            {
                text += "We found that this article was processed with one or more mistakes. Review below.";

                // Check if hidden image exists. If so, give warning if it wasn't found, or if it was found and article not banned
                if (article.hiddenImageExists)
                    if (!article.hiddenImageFound || (article.hiddenImageFound && !article.articleBanned))
                        text += "\n\nWe located a image hidden within this article. Ensure that future articles are scoured diligently and banned accordingly.";

                // Check if article should have been banned or was banned mistakenly
                if (article.bannedWords.Length > 0 && !article.articleBanned)
                {
                    text += "\n\nThe publisher of this article is untruthful and a hazard to NewMerica. Always ban articles from this publisher.\nBe on the look out for future articles from:\n";
                    for (int i = 0; i < article.bannedWords.Length; i++)
                    {
                        if (i > 0 && i == article.bannedWords.Length - 1)
                            text += " or ";
                        else if (i > 0)
                            text += ", ";

                        text += $"{article.bannedWords[i]}";
                    }
                    text += ".";
                }
                else if (article.bannedWords.Length == 0 && article.articleBanned)
                    text += "\n\nThis article was from a reputable source. Ensure only articles from select publishers are banned.";

                // Check words that were not censored or words that were censored.
                if (article.numCensorableWords != article.numCensoredCorrectly)
                {
                    text += $"\n\nYou missed {article.numCensorableWords - article.numCensoredCorrectly} word(s) that should have censored. We have provided the list of censored words found on this article. Ensure you remember them:\n";
                    for (int i = 0; i < article.censorWords.Length; i++)
                    {
                        if (i > 0 && i == article.censorWords.Length - 1)
                            text += " and ";
                        else if (i > 0)
                            text += ", ";

                        text += $"{article.censorWords[i]}";
                    }
                    text += ".";
                }
                if (article.numCensorMistakes > 0)
                    text += $"\n\nWe have found that you censored {article.numCensorMistakes} word(s) that should not have been censored. Censoring incorrect words can lead to untruths which confuse readers and can sow disorder. Do not censor words according to your own discretion.";
            }
            text += $"\n\nYour resulting pay for this article was {article.moneyEarned}.";
            if (article.OverTime)
                text += $"\nYour pay was decreased for working overtime. Overtime pay is not permitted. Ensure you work more effciently next time.";
            text += $"\n\nTitle: {article.title}\nPublisher: {article.publisher}\nDate: {article.date}\n{article.body}";
            SetObjectText(reviewText, text);
        }
        else
            SetObjectText(reviewText, "");
    }

    private void ReviewCountUpdate(int num)
    {
        unreadReviewCount += num;
        UpdateUnreadPopUps();
    }

    internal void EventTrigger(int day, bool startEnd)
    {
        if (day == 3)
        {
            ShowHideImage(foreground, startEnd);
            foreground.sprite = glitchedScreen;
            if (lastOpenedScreen != null)
                lastOpenedScreen.SetActive(!startEnd);
        }
    }

    private bool zoomedIn = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (!zoomedIn)
            {
                EventManager.ZoomCamera?.Invoke(screenZoomIn, 3.1f, 0.25f);
                zoomedIn = true;
            }
            else
            {
                EventManager.ResetCamera?.Invoke(0.25f);
                zoomedIn = false;
            }
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
