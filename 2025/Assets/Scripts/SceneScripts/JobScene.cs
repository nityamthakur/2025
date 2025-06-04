using UnityEngine.UI;
using UnityEngine;
using System.Collections;
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
    [SerializeField] private CosmeticObject[] allCosmeticObjects;
    private GameObject currJobScene;
    private Image backgroundImage, dropBoxAcceptGlow;
    private bool jobDelayed;
    // ---------------------------------
    [SerializeField] private GameObject jobBuildingPrefab;
    [SerializeField] private Sprite jobBuildingImage;
    private GameObject outsideBuildingObject;
    private Canvas canvas, prefabCanvas;
    // ---------------------------------
    private float workTimer = 150f;
    private GameObject computerScreenPrefab;
    private ComputerScreen computerScreenClass;

    // ---------------------------------

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
    }

    private float GetWorkTimer()
    {
        return workTimer + gameManager.gameData.GetTimerUpgrade();
    }

    public void LoadJobStart()
    {
        ShowBuildingTransition();
        //objectSpawner.SpawnRentNotice();
        LoadJsonFromFile();
        SetUpJobStart();
        computerScreenClass.StartComputer();
        EventManager.ShowHideRentNotices?.Invoke(true);
        EventManager.FadeIn?.Invoke();
        EventManager.PlayMusic?.Invoke("work");
    }

    private void CanvasChanger(bool change)
    {
        if (canvas != null && !change)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
        }
        else if (canvas != null && change)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
        }
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
        prefabCanvas = outsideBuildingObject.GetComponentInChildren<Canvas>();
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


    private void SetUpJobStart()
    {
        currJobScene = Instantiate(jobScenePrefab);
        if (currJobScene == null)
        {
            Debug.LogError("currJobScene object is null in LoadJobStart of JobScene class.");
            return;
        }

        canvas = currJobScene.GetComponentInChildren<Canvas>();
        if (canvas == null && mainCamera == null)
        {
            Debug.LogError("Failed to set Event Camera. Canvas or mainCamera is missing.");
        }
        else
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
        }

        ComputerScreenSetUp();

        backgroundImage = currJobScene.transform.Find("BackgroundImage").GetComponent<Image>();
        if (backgroundImage == null)
        {
            Debug.Log("Failed to find BackgroundImage in SetUpJobStart");
            return;
        }
        backgroundImage.sprite = workBackgroundImage;

        Canvas overlayCanvas = currJobScene.transform.Find("OverlayCanvas").GetComponent<Canvas>();
        dropBoxAcceptGlow = overlayCanvas.transform.Find("DropBoxAcceptGlow").GetComponent<Image>();
        if (dropBoxAcceptGlow == null)
        {
            Debug.Log("Failed to find DropBoxAcceptGlow in SetUpJobStart");
            return;
        }

        allCosmeticObjects = currJobScene.GetComponentsInChildren<CosmeticObject>(true);
        foreach (var cosmetic in allCosmeticObjects)
        {
            bool owned = gameManager.gameData.IsCosmeticPurchased(cosmetic.id);
            cosmetic.gameObject.SetActive(owned);
        }

        StartCoroutine(PulseGlow(dropBoxAcceptGlow));
        dropBoxAcceptGlow.gameObject.SetActive(false);
    }

    public void RefreshCosmetics()
    {
        if (allCosmeticObjects == null) return;
        foreach (var cosmetic in allCosmeticObjects)
        {
            if (cosmetic == null) continue;
            bool owned = gameManager.gameData.IsCosmeticPurchased(cosmetic.id);
            cosmetic.gameObject.SetActive(owned);
        }
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
        if (computerScreenClass == null)
        {
            Debug.Log("Failed to find ComputerScreenClass in SetUpJobStart");
            return;
        }
        computerScreenClass.Initalize();
        computerScreenClass.CreateEmails(gameManager.gameData.releasedEmails);
        computerScreenClass.CreateReviews(gameManager.gameData.articleReviews, gameManager.gameData.GetCurrentDay());
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

    public IEnumerator BeginWorkDay()
    {
        yield return StartCoroutine(CheckDailyEvent());
        gameManager.SetJobScene(this);
        objectSpawner.StartMediaSpawn();
        gameManager.StartJobTimer(GetWorkTimer());
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

        string performanceText = $"Day {gameManager.gameData.day} Results:\n\nMedia Processed: {mediaProcessed}\n\nSupervisors Notified of Your Day\n\nProfit: ${score}\n\nPossibility of Promotion: {promotionPossibility}";
        computerScreenClass.SetScreenText(performanceText);
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
        EventManager.DisplayLightsOutImage?.Invoke(false);

        Destroy(currJobScene);
        currJobScene = null;
        EventManager.ShowHideRentNotices?.Invoke(false);
        EventManager.ResetCamera?.Invoke(0f);


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
        EventManager.CameraZoomed += CanvasChanger;
    }

    private void OnDisable()
    {
        EventManager.OnImageDestroyed -= HandleImageDestroyed;
        EventManager.GlowingBoxShow -= GlowingBoxShow;
        EventManager.CameraZoomed -= CanvasChanger;
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