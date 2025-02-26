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
    private GameObject currJobScene;
    private Image backgroundImage;
    private Button startWorkButton;
    private TextMeshProUGUI screenText;
    private string currentEmail;
    
    // ---------------------------------
    [SerializeField] private GameObject jobBuildingPrefab;
    [SerializeField] private Sprite jobBuildingImage;
    private GameObject outsideBuildingObject;

    // ---------------------------------

    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void LoadJobStart(int day) {

        //ShowBuildingTransition();
        LoadJsonFromFile();
        SetUpJobStart(day);
        EventManager.FadeIn?.Invoke(); 
    }

    private void ShowBuildingTransition()
    {
        Debug.Log("Starting Building Transition");
        outsideBuildingObject = Instantiate(jobBuildingPrefab);
        if (outsideBuildingObject == null)
        {
            Debug.LogError("outsideBuildingObject object is null in ShowBuildingTransition.");
            return;
        }

        backgroundImage = outsideBuildingObject.transform.Find("BackgroundImage").GetComponent<Image>();
        if(backgroundImage == null)
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
    }

    private void SetUpJobStart(int day) {
        Debug.Log("Setting up Job Start");

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
        if(backgroundImage == null)
        {
            Debug.Log("Failed to find BackgroundImage in SetUpJobStart");
            return;
        }
        backgroundImage.sprite = workBackgroundImage; 

        startWorkButton = currJobScene.transform.Find("WorkButton").GetComponent<Button>();
        if (startWorkButton == null)
        {
            Debug.LogError("Failed to find startWorkButton component in SetUpJobStart.");
            return;
        }
        startWorkButton.onClick.AddListener(() =>
        {
            startWorkButton.interactable = false; // Disable immediately
            BeginWorkDay();
        });

        screenText = currJobScene.transform.Find("ComputerScreenText").GetComponent<TextMeshProUGUI>();
        if (screenText == null)
        {
            Debug.LogError("Failed to find screenText component in ShowResults.");
            return;
        }
        SetScreenEmail(screenText);
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
        if (gameManager.GetCurrentDay() == 1) return;

        screenText.text += "\nCensor List:\n";
        foreach (string censor in gameManager.GetCensorTargetWords())
        {
            screenText.text += censor + "\n";
        }
    }

    private void BeginWorkDay(){
        gameManager.SetJobScene(this);
        objectSpawner.StartMediaSpawn();
        SetScreenObjectives(screenText);
        startWorkButton.gameObject.SetActive(false);
    }

    public void ShowResults(int day, int mediaProcessed, int score) {
        TextMeshProUGUI buttonText = startWorkButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null) {
            buttonText.text = "End Day";
        } else {
            Debug.LogError("TextMeshProUGUI component not found on startWorkButton.");
        }

        startWorkButton.onClick.RemoveAllListeners();
        startWorkButton.interactable = true;
        startWorkButton.gameObject.SetActive(true);
        startWorkButton.onClick.AddListener(() =>
        {
            startWorkButton.interactable = false; // Disable immediately
            StartCoroutine(NextScene());
        });
        screenText.text = $"Day {day} Results:\n\nMedia Processed: {mediaProcessed}\n\nSupervisors Notified of Your Day\n\nProfit: ${score}\n\nPossibility of Promotion: Unknown";

        // Set the results text based on the job details
    }

    private IEnumerator NextScene()
    {
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
            currentEmail = GetEmailForDay(jsonObject.emailText, gameManager.GetCurrentDay());
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


