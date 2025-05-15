using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using TMPro;
using System.IO;
using System.Collections.Generic;
using System;

public class JobScene : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private ObjectSpawner objectSpawner;
    [SerializeField] private GameObject jobScenePrefab;
    [SerializeField] private Sprite workBackgroundImage;
    [SerializeField] private Sprite glitchedScreen;

    private GameObject currJobScene;
    private Image backgroundImage, dropBoxAcceptGlow, dropBoxDestroyGlow;
    private bool jobDelayed;
    // ---------------------------------
    [SerializeField] private GameObject jobBuildingPrefab;
    [SerializeField] private Sprite jobBuildingImage;
    private GameObject outsideBuildingObject;

    // ---------------------------------
    private float workTimer = 150f;
    [SerializeField] private float baseWorkTimer = 150f;
    [SerializeField] private float timerUpgradeBonus = 50f;
    private GameObject computerScreenPrefab;
    private ComputerScreen computerScreenClass;

    // ---------------------------------
    //WANT TODO: Update Clock hand sprites to be square and have time move in ticking increments instead of smooth
    private Image hourHand;
    private Image minuteHand;
    private int dayProfit = 0;
    public int DayProfit
    {
        get { return dayProfit; }
        private set { dayProfit = value; }
    }

    // ---------------------------------

    public GameManager gameManager;

    public void Initialize()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        UpdateWorkTimer();
    }

    private void UpdateWorkTimer()
    {
        if (gameManager != null && gameManager.gameData != null && gameManager.gameData.HasTimerUpgrade())
        {
            workTimer = baseWorkTimer + timerUpgradeBonus;
        }
        else
        {
            workTimer = baseWorkTimer;
        }
    }


    public void LoadJobStart() {
        //ShowBuildingTransition();                                 uncomment
        LoadJsonFromFile();
        SetUpJobStart();
        computerScreenClass.StartComputer();
        EventManager.ShowHideRentNotices?.Invoke(true);
        EventManager.FadeIn?.Invoke();
        EventManager.PlayMusic?.Invoke("work");
    }

    private void ShowBuildingTransition()
    {
        //Debug.Log("Starting Building Transition");
        outsideBuildingObject = Instantiate(jobBuildingPrefab);
        if (outsideBuildingObject == null)
        {
            Debug.LogError("outsideBuildingObject object is null in ShowBuildingTransition.");
            return;
        }
        Canvas prefabCanvas = outsideBuildingObject.GetComponentInChildren<Canvas>();
        if (prefabCanvas != null)
        {
            prefabCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            prefabCanvas.worldCamera = Camera.main;
        }


        backgroundImage = outsideBuildingObject.transform.Find("BackgroundImage").GetComponent<Image>();
        if (backgroundImage == null)
        {
            Debug.Log("Failed to find BackgroundImage in ShowBuildingTransition.");
            return;
        }
        backgroundImage.sprite = jobBuildingImage;

        StartCoroutine(TransitionBuildingFade());
    }

    private IEnumerator TransitionBuildingFade()
    {
        EventManager.FadeIn?.Invoke();
        yield return new WaitForSeconds(2f);

        // Press to Continue or Continue Automatically?
        yield return new WaitForSeconds(2f);

        EventManager.FadeOut?.Invoke();
        yield return new WaitForSeconds(2f);
        Destroy(outsideBuildingObject);
        outsideBuildingObject = null;
        yield return new WaitForSeconds(0f);

        EventManager.FadeIn?.Invoke();
        EventManager.DisplayMenuButton?.Invoke(true);

        yield return new WaitForSeconds(2f);
        objectSpawner.SpawnRentNotice();
    }


    private void SetUpJobStart() {
        currJobScene = Instantiate(jobScenePrefab);
        if (currJobScene == null)
        {
            Debug.LogError("currJobScene object is null in LoadJobStart of JobScene class.");
            return;
        }

        Canvas canvas = currJobScene.GetComponentInChildren<Canvas>();
        if (canvas == null && mainCamera == null)
        {
            Debug.LogError("Failed to set Event Camera. Canvas or mainCamera is missing.");
        }
        else
        {
            canvas.worldCamera = mainCamera.GetComponent<Camera>();
        }

        ComputerScreenSetUp();

        backgroundImage = currJobScene.transform.Find("BackgroundImage").GetComponent<Image>();
        if (backgroundImage == null)
        {
            Debug.Log("Failed to find BackgroundImage in SetUpJobStart");
            return;
        }
        backgroundImage.sprite = workBackgroundImage;

        hourHand = currJobScene.transform.Find("HourHand").GetComponent<Image>();
        if (hourHand == null)
        {
            Debug.Log("Failed to find hourHand in SetUpJobStart");
        }

        dropBoxAcceptGlow = currJobScene.transform.Find("DropBoxAcceptGlow").GetComponent<Image>();
        if (dropBoxAcceptGlow == null)
        {
            Debug.Log("Failed to find DropBoxAcceptGlow in SetUpJobStart");
            return;
        }
        StartCoroutine(PulseGlow(dropBoxAcceptGlow));
        dropBoxAcceptGlow.gameObject.SetActive(false);

        dropBoxDestroyGlow = currJobScene.transform.Find("DropBoxDestroyGlow").GetComponent<Image>();
        if (dropBoxDestroyGlow == null)
        {
            Debug.Log("Failed to find DropBoxDestroyGlow in SetUpJobStart");
            return;
        }
        StartCoroutine(PulseGlow(dropBoxDestroyGlow));
        dropBoxDestroyGlow.gameObject.SetActive(false);
    }

    private void ComputerScreenSetUp()
    {
        Transform screenTransform = currJobScene.transform.Find("ComputerScreen");
        if (screenTransform == null)
        {
            Debug.LogError("Could not find 'ComputerScreen' under currJobScene.");
            return;
        }
        computerScreenPrefab = screenTransform.gameObject;

        computerScreenClass = computerScreenPrefab.GetComponent<ComputerScreen>();
        if(computerScreenClass == null)
        {
            Debug.Log("Failed to find ComputerScreenClass in SetUpJobStart");
            return;
        }
        computerScreenClass.Initalize();
        computerScreenClass.CreateEmails(gameManager.gameData.releasedEmails);
    }

    private void SetScreenObjectives(TextMeshProUGUI screenText)
    {
        string text = "Ban List:\n";
        foreach (string ban in gameManager.GetBanTargetWords())
        {
            text += ban + "\n";
        }

        // Don't show the censor list on the first day
        if (gameManager.gameData.GetCurrentDay() == 1) return;

        text += "\nCensor List:\n";
        foreach (string censor in gameManager.GetCensorTargetWords())
        {
            text += censor + "\n";
        }

        computerScreenClass.SetScreenText(text);
    }
    

    public IEnumerator BeginWorkDay()
    {
        yield return StartCoroutine(CheckDailyEvent());
        gameManager.SetJobScene(this);
        objectSpawner.StartMediaSpawn();
        UpdateWorkTimer();
        gameManager.StartJobTimer(workTimer);
    }

    public void ShowResults(int mediaProcessed, int score)
    {
        computerScreenClass.EndDaySetUp();

        dayProfit = score;
        computerScreenClass.SetPerformanceSliderValue(gameManager.gameData.PerformanceScale);

        string promotionPossibility = "Unknown";
        if (gameManager.gameData.PerformanceScale >= 0.66f)
        {
            promotionPossibility = "Likely";
        }
        else if (gameManager.gameData.PerformanceScale < 0.33f)
        {
            promotionPossibility = "Unlikely";
        }

        string performanceText = $"Day {gameManager.gameData.day} Results:\n\nMedia Processed: {mediaProcessed}\n\nSupervisors Notified of Your Day\n\nProfit: ${score}\nCurrent Savings: ${gameManager.gameData.GetCurrentMoney()}\n\nPossibility of Promotion: {promotionPossibility}";
        computerScreenClass.SetScreenText(performanceText);
    }

    public void UpdateClockHands(float progress)
    {
        //WANT TODO: Update Clock hand sprites to be square and have time move in ticking increments instead of smooth
        if (hourHand)
        {
            float hourRotation = Mathf.Lerp(0f, 180f, progress); // Moves from 0 to 180 degrees
            hourHand.transform.eulerAngles = new Vector3(0, 0, -hourRotation); // Invert rotation for correct direction
        }

        if (minuteHand)
        {
            float minuteRotation = Mathf.Lerp(0f, 2880f, progress); // 8 full revolutions (8 × 360)
            minuteHand.transform.eulerAngles = new Vector3(0, 0, -minuteRotation);
        }
    }

    public void UpdateMediaProcessedText(int num)
    {
        computerScreenClass.SetProcessedText($"MEDIA PROCESSED:\n{num} / 5");
    }

    public IEnumerator NextScene()
    {
        EventManager.DisplayMenuButton?.Invoke(false);
        EventManager.FadeOut?.Invoke();
        yield return new WaitForSeconds(2f);
        EventManager.HideLightsOutImage?.Invoke();

        Destroy(currJobScene);
        currJobScene = null;
        EventManager.ShowHideRentNotices?.Invoke(false);

        yield return new WaitForSeconds(2f);
        EventManager.NextScene?.Invoke();
    }

    private void LoadJsonFromFile()
    {
        // Check if Json is found in StreamingAssets folder
        string path = Path.Combine(Application.streamingAssetsPath, "GameText.json");
        if (!File.Exists(path))
        {
            Debug.LogError("JSON file not found at: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        ParseJson(json);
    }

    private void ParseJson(string json)
    {
        var jsonObject = JsonUtility.FromJson<Wrapper>(json);

        if (jsonObject != null && jsonObject.emailText.Count > 0)
        {
            GetEmailForDay(jsonObject.emailText, gameManager.gameData.GetCurrentDay());
        }
        else
        {
            Debug.LogError("JSON parsing failed or empty list.");
        }
    }

    private void GetEmailForDay(List<Entry> entries, int day)
    {
        foreach (var entry in entries)
        {
            if (entry.day == day)
            {
                gameManager.gameData.releasedEmails.Add(entry);
                return;
            }
        }
        return;
    }

    private IEnumerator CheckDailyEvent()
    {
        if (gameManager.gameData.GetCurrentDay() == 3)
        {
            EventManager.ShowCustomSubtitle?.Invoke("Music pausing for dramatic effect");
            EventManager.PauseResumeMusic?.Invoke();
            jobDelayed = true;

            // Pause for effect
            yield return new WaitForSeconds(3f);
            
            EventManager.PlaySound?.Invoke("glitch", true);

            computerScreenClass.EventTrigger(3, jobDelayed);
            objectSpawner.SpawnImageObject(true);

            // Prevent progression
            yield return new WaitUntil(() => !jobDelayed);

            EventManager.PlaySound?.Invoke("glitch", true); 
            yield return new WaitForSeconds(2.5f);

            computerScreenClass.EventTrigger(3, jobDelayed);
            EventManager.PauseResumeMusic?.Invoke(); 
        }
    }

    private void GlowingBoxShow(string Box, bool show)
    {
        if (Box.ToLower() == "accept")
            dropBoxAcceptGlow.gameObject.SetActive(show);
        else if (Box.ToLower() == "destroy")
            dropBoxDestroyGlow.gameObject.SetActive(show);
    }

    private IEnumerator PulseGlow(Image glowImage)
    {
        float duration = 1.0f; // Time for one full cycle (fade in and out)
        float alphaMin = 0.3f; // Minimum transparency
        float alphaMax = 1.0f; // Maximum transparency

        while (glowImage != null)
        {
            // Fade in
            for (float t = 0; t < duration / 2; t += Time.deltaTime)
            {
                if (glowImage == null) yield break;
                float alpha = Mathf.Lerp(alphaMin, alphaMax, t / (duration / 2));
                glowImage.color = new Color(glowImage.color.r, glowImage.color.g, glowImage.color.b, alpha);
                yield return null;
            }

            // Fade out
            for (float t = 0; t < duration / 2; t += Time.deltaTime)
            {
                if (glowImage == null) yield break;
                float alpha = Mathf.Lerp(alphaMax, alphaMin, t / (duration / 2));
                glowImage.color = new Color(glowImage.color.r, glowImage.color.g, glowImage.color.b, alpha);
                yield return null;
            }
        }
    }

    // EventManager for continuing gameplay on an ImageObject being destroyed
    private void OnEnable()
    {
        EventManager.OnImageDestroyed += HandleImageDestroyed;
        EventManager.GlowingBoxShow += GlowingBoxShow;
    }

    private void OnDisable()
    {
        EventManager.OnImageDestroyed -= HandleImageDestroyed;
        EventManager.GlowingBoxShow -= GlowingBoxShow;
    }

    private void HandleImageDestroyed()
    {
        jobDelayed = false;
    }

    [Serializable]
    public class Wrapper
    {
        public List<Entry> emailText;
    }

    [Serializable]
    public class Entry
    {
        public bool seen = false;
        public int day;
        public string title;
        public string sender;
        public string email;
    }
}