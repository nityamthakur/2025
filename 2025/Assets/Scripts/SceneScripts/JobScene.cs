using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using TMPro;
using System.IO;
using System.Collections.Generic;
using System;
using JetBrains.Annotations;

public class JobScene : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private ObjectSpawner objectSpawner;
    [SerializeField] private GameObject jobScenePrefab;
    [SerializeField] private Sprite workBackgroundImage;
    [SerializeField] private Sprite glitchedScreen;

    private GameObject currJobScene;
    private Image backgroundImage;
    private Image computerScreen;
    private Button startWorkButton;
    private TextMeshProUGUI screenText;
    private TextMeshProUGUI mediaProcessedText;
    private string currentEmail;
    private bool jobDelayed;
    // ---------------------------------
    [SerializeField] private GameObject jobBuildingPrefab;
    [SerializeField] private Sprite jobBuildingImage;
    private GameObject outsideBuildingObject;

    // ---------------------------------
    private float workTimer = 180f;      // 180f
    private Image hourHand;
    private Image minuteHand;
    private Slider performanceSlider;

    // ---------------------------------

    private GameManager gameManager;

    public void Initialize()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }


    public void LoadJobStart() {
        ShowBuildingTransition();
        LoadJsonFromFile();
        SetUpJobStart();
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
        //Debug.Log("Setting up Job Start");
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

        backgroundImage = currJobScene.transform.Find("BackgroundImage").GetComponent<Image>();
        if (backgroundImage == null)
        {
            Debug.Log("Failed to find BackgroundImage in SetUpJobStart");
            return;
        }
        backgroundImage.sprite = workBackgroundImage;

        computerScreen = currJobScene.transform.Find("ComputerScreenImage").GetComponent<Image>();
        if(computerScreen == null)
        {
            Debug.Log("Failed to find ComputerScreenImage in SetUpJobStart");
            return;
        }
        computerScreen.gameObject.SetActive(false); 

        startWorkButton = currJobScene.transform.Find("WorkButton").GetComponent<Button>();
        if (startWorkButton == null)
        {
            Debug.LogError("Failed to find startWorkButton component in SetUpJobStart.");
            return;
        }
        startWorkButton.onClick.AddListener(() =>
        {
            startWorkButton.gameObject.SetActive(false);
            EventManager.PlaySound?.Invoke("switch1");
            StartCoroutine(BeginWorkDay());
        });

        screenText = currJobScene.transform.Find("ComputerScreenText").GetComponent<TextMeshProUGUI>();
        if (screenText == null)
        {
            Debug.LogError("Failed to find screenText component in ShowResults.");
            return;
        }
        SetScreenEmail(screenText);

        mediaProcessedText = currJobScene.transform.Find("MediaProcessedText").GetComponent<TextMeshProUGUI>();
        if (screenText == null)
        {
            Debug.LogError("Failed to find mediaProcessedText component in ShowResults.");
            return;
        }
        ShowMediaProcessedText(false);

        hourHand = currJobScene.transform.Find("HourHand").GetComponent<Image>();
        if (hourHand == null)
        {
            Debug.Log("Failed to find hourHand in SetUpJobStart");
        }

        minuteHand = currJobScene.transform.Find("MinuteHand").GetComponent<Image>();
        if (minuteHand == null)
        {
            Debug.Log("Failed to find minuteHand in SetUpJobStart");
        }

        performanceSlider = currJobScene.transform.Find("PerformanceScale").GetComponent<Slider>();
        if (performanceSlider == null)
        {
            Debug.Log("Failed to find performanceSlider in SetUpJobStart");
        }
        performanceSlider.gameObject.SetActive(false);
    }

    private void SetScreenEmail(TextMeshProUGUI screenText)
    {
        screenText.text = currentEmail;
    }

    private void SetScreenObjectives(TextMeshProUGUI screenText)
    {
        screenText.text = "Ban List:\n";
        foreach (string ban in gameManager.GetBanTargetWords())
        {
            screenText.text += ban + "\n";
        }

        // Don't show the censor list on the first day
        if (gameManager.gameData.GetCurrentDay() == 1) return;

        screenText.text += "\nCensor List:\n";
        foreach (string censor in gameManager.GetCensorTargetWords())
        {
            screenText.text += censor + "\n";
        }
    }


    private IEnumerator BeginWorkDay()
    {
        yield return StartCoroutine(CheckDailyEvent()); 
        gameManager.SetJobScene(this);
        objectSpawner.StartMediaSpawn();        
        SetScreenObjectives(screenText);
        ShowMediaProcessedText(true);
        gameManager.StartJobTimer(workTimer); // Start the game timer
    }

    public void ShowResults(int mediaProcessed, int score)
    {
        TextMeshProUGUI buttonText = startWorkButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = "End Day";
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found on startWorkButton.");
        }
        performanceSlider.gameObject.SetActive(true);
        performanceSlider.value = gameManager.gameData.PerformanceScale;

        startWorkButton.onClick.RemoveAllListeners();
        startWorkButton.interactable = true;
        startWorkButton.gameObject.SetActive(true);
        startWorkButton.onClick.AddListener(() =>
        {
            startWorkButton.interactable = false;
            gameManager.gameData.money += score;
            StartCoroutine(NextScene());
        });

        string promotionPossibility = "Unknown";
        if (gameManager.gameData.PerformanceScale >= 0.66f)
        {
            promotionPossibility = "Likely";
        }
        else if (gameManager.gameData.PerformanceScale < 0.33f)
        {
            promotionPossibility = "Unlikely";
        }

        screenText.text = $"Day {gameManager.gameData.day} Results:\n\nMedia Processed: {mediaProcessed}\n\nSupervisors Notified of Your Day\n\nProfit: ${score}\n\nTotal Money: ${gameManager.gameData.money + score}\n\nPossibility of Promotion: {promotionPossibility}";

    }

    public void UpdateClockHands(float progress)
    {
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
        if (mediaProcessedText != null)
            mediaProcessedText.text = $"Media Processed:\n{num} / 5";
    }

    public void ShowMediaProcessedText(bool show)
    {
        if (mediaProcessedText != null)
            mediaProcessedText.enabled = show;
    }

    private IEnumerator NextScene()
    {
        EventManager.DisplayMenuButton?.Invoke(false);
        EventManager.StopMusic?.Invoke();
        EventManager.FadeOut?.Invoke();
        yield return new WaitForSeconds(2f);

        Destroy(currJobScene);
        currJobScene = null;

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
            currentEmail = GetEmailForDay(jsonObject.emailText, gameManager.gameData.GetCurrentDay());
        }
        else
        {
            Debug.LogError("JSON parsing failed or empty list.");
        }
    }

    private string GetEmailForDay(List<Entry> entries, int day)
    {
        foreach (var entry in entries)
        {
            if (entry.day == day)
            {
                return entry.email;
            }
        }
        return string.Empty;
    }

    private IEnumerator CheckDailyEvent()
    {
        if(gameManager.gameData.GetCurrentDay() == 3)
        {
            EventManager.ShowCustomSubtitle?.Invoke("Music pausing for dramatic effect"); 
            EventManager.PauseResumeMusic?.Invoke(); 

            jobDelayed = true;
            // Pause for effect
            yield return new WaitForSeconds(3f);
            EventManager.PlaySound?.Invoke("glitch"); 

            screenText.gameObject.SetActive(false);
            computerScreen.gameObject.SetActive(true);
            computerScreen.sprite = glitchedScreen;

            objectSpawner.SpawnImageObject(true);
            // Prevent progression
            yield return new WaitUntil(() => !jobDelayed);
            
            EventManager.PlaySound?.Invoke("glitch"); 
            yield return new WaitForSeconds(2.5f);

            computerScreen.gameObject.SetActive(false);
            screenText.gameObject.SetActive(true);
            EventManager.PauseResumeMusic?.Invoke(); 
        }
    }

   // EventManager for continuing gameplay on an ImageObject being destroyed
    private void OnEnable()
    {
        EventManager.OnImageDestroyed += HandleImageDestroyed;
    }

    private void OnDisable()
    {
        EventManager.OnImageDestroyed -= HandleImageDestroyed;
    }

    private void HandleImageDestroyed()
    {
        jobDelayed = false;
    }


    [Serializable]
    private class Wrapper
    {
        public List<Entry> emailText;
    }

    [Serializable]
    private class Entry
    {
        public int day;
        public string email;
    }
}


