using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Collections;

public class JobScene : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private ObjectSpawner objectSpawner;
    [SerializeField] private GameObject jobScenePrefab;
    [SerializeField] private Sprite workBackgroundImage;
    private GameObject currJobScene;
    private Image backgroundImage;
    private Button startWorkButton;

    [SerializeField] private GameObject jobBuildingPrefab;
    [SerializeField] private Sprite jobBuildingImage;
    private GameObject outsideBuildingObject;

    private int currDay = 0;

    // For using a media target goal for the day
    private int numMediaProcessed = 0; 
    private int numMediaNeeded = 10;

    // For using a time based system for the day
    private int currClockTime = 0;
    private int clockTimeJobEnd = 1000;

    // Idea to consider. Pass in the parameters of Day and Reputation with the two parties
    // and other internal values as a class so that they can be used instead of multiple variables

    public void LoadJobStart(int day) {

        ShowBuildingTransition();
        SetUpJobStart(day);
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
        yield return new WaitForSeconds(2f);
        EventManager.FadeIn?.Invoke(); 
    }

    private void SetUpJobStart(int day) {
        Debug.Log("Setting up Job Start");

        currDay = day;
        numMediaProcessed = 0;
        numMediaNeeded = 10;
        currClockTime = 0;
        clockTimeJobEnd = 1000;
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
        startWorkButton.onClick.AddListener(BeginWorkDay);
    }

    private void BeginWorkDay(){
        objectSpawner.StartMediaSpawn();
        startWorkButton.gameObject.SetActive(false);
        ShowResults();
    }

    private void ProcessMediaObject()
    {
        // Check for Spawning a NewMedia Object or Moving to Next Scene / Job Day End
        // Destroy UI object or hide.
        Debug.Log("End of dialogue.");
        StartCoroutine(NextScene());
    }

    private void ShowResults() {
        TextMeshProUGUI buttonText = startWorkButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null) {
            buttonText.text = "Continue";  // Set the desired text
        } else {
            Debug.LogError("TextMeshProUGUI component not found on startWorkButton.");
        }

        startWorkButton.onClick.RemoveAllListeners();
        startWorkButton.onClick.AddListener(() => StartCoroutine(NextScene()));
        startWorkButton.gameObject.SetActive(true);
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
}
