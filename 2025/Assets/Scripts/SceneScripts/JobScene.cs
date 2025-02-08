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

        EventManager.FadeIn?.Invoke(); 
        ShowBuildingTransition();
        EventManager.FadeOut?.Invoke(); 
        SetUpJobStart(day);
        EventManager.FadeIn?.Invoke(); 
        objectSpawner.SpawnNewMediaObject();
    }

    private void ShowBuildingTransition()
    {
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
        backgroundImage.sprite = workBackgroundImage; 

        StartCoroutine(TransitionBuildingFade());
    }

    private IEnumerator TransitionBuildingFade()
    {
        EventManager.FadeIn?.Invoke();
        yield return new WaitForSeconds(2f);

        Destroy(outsideBuildingObject);
        outsideBuildingObject = null;
        
        yield return new WaitForSeconds(2f);
    }


    private void SetUpJobStart(int day) {
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

        // ✅ Assign the main camera to the Event Camera of the Canvas
        Canvas canvas = currJobScene.GetComponentInChildren<Canvas>();
        if (canvas != null && mainCamera != null)
        {
            canvas.worldCamera = mainCamera.GetComponent<Camera>(); // ✅ Assign the camera
            Debug.Log($"🎥 Event Camera set for currJobScene: {mainCamera.name}");
        }
        else
        {
            Debug.LogError("❌ Failed to set Event Camera. Canvas or mainCamera is missing.");
        }


        backgroundImage = currJobScene.transform.Find("BackgroundImage").GetComponent<Image>();
        if(backgroundImage == null)
        {
            Debug.Log("Failed to find BackgroundImage in SetUpJobStart");
            return;
        }
        backgroundImage.sprite = workBackgroundImage; 

    }

    private void ProcessMediaObject()
    {
        // Check for Spawning a NewMedia Object or Moving to Next Scene / Job Day End
        // Destroy UI object or hide.
        Debug.Log("End of dialogue.");
        StartCoroutine(NextScene());
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
