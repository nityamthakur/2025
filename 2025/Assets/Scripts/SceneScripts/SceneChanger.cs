using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class SceneChanger : MonoBehaviour
{
    public SceneChanger Instance { get; private set; }
    [SerializeField] private GameManager gameManager;
    [SerializeField] private MainMenuScene mainMenuScene;
    [SerializeField] private DayStartScene dayStartScene;
    [SerializeField] private JobScene jobScene;
    [SerializeField] private GameObject fadingScreenPrefab;
    private GameObject fadingScreen;
    private Image fadingImage;
    private Image deskOverlayImage;

    private int currentSceneIndex = 0;
    private List<Action> sceneSequence;

    void Start()
    {
        if(gameManager == null)
        {
            Debug.Log("gameManager is null in SceneChanger");
        }
        // Define the order of the scenes
        sceneSequence = new List<Action>
        {
            () => dayStartScene.LoadDayStart(gameManager.GetCurrentDay()),
            () => jobScene.LoadJobStart(gameManager.GetCurrentDay()),
        };
        fadingScreen = Instantiate(fadingScreenPrefab);

        if (fadingScreen == null)
        {
            Debug.LogError("fadingImage is null in SceneManager.");
            return;
        }

        fadingImage = fadingScreen.transform.Find("FadingImage").GetComponent<Image>();
        if (fadingImage == null)
        {
            Debug.LogError("Failed to find FadingImage component in SceneManager");
            return;
        }

        // Used for ensuring the media gets enters and leaves behind certain screen elements
        deskOverlayImage = fadingScreen.transform.Find("DeskOverlay").GetComponent<Image>();
        if (deskOverlayImage == null)
        {
            Debug.LogError("Failed to find DeskOverlay component in SceneManager");
            return;
        }
        deskOverlayImage.gameObject.SetActive(false);

        mainMenuScene.LoadMainMenu();
        //jobScene.LoadJobStart(gameManager.GetCurrentDay());

    }

    public void StartNextScene()
    {
        // Call the function for the current scene
        sceneSequence[currentSceneIndex]?.Invoke();

        // Increment and loop back if at the end
        currentSceneIndex = (currentSceneIndex + 1) % sceneSequence.Count;
    }

    void OnEnable()
    {
        EventManager.NextScene += StartNextScene;
        EventManager.FadeIn += FadeInScreen;
        EventManager.FadeOut += FadeOutScreen;
        EventManager.ShowDeskOverlay += ShowDeskOverLay;
        EventManager.HideDeskOverlay += HideDeskOverLay;
    }

    void OnDisable()
    {
        EventManager.NextScene -= StartNextScene;
        EventManager.FadeIn -= FadeInScreen;
        EventManager.FadeOut -= FadeOutScreen;
        EventManager.ShowDeskOverlay -= ShowDeskOverLay;
        EventManager.HideDeskOverlay -= HideDeskOverLay;
    }

    private void FadeInScreen()
    {
        StartCoroutine(FadeIn());
    }

    private void FadeOutScreen()
    {
        StartCoroutine(FadeOut());
    }

    private float fadeDuration = 1f; // Adjustable fade duration
    //private float waitTime = 1f; // Time to wait before fading back in
    private IEnumerator FadeIn()
    {
        yield return StartCoroutine(FadeImage(fadingImage, 1f, 0f, fadeDuration)); // fadeInOut goes to 100% opacity (Black Screen) 
        fadingImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeOut()
    {
        fadingImage.gameObject.SetActive(true);   
        yield return StartCoroutine(FadeImage(fadingImage, 0f, 1f, fadeDuration)); // fadeInOut goes to 100% opacity (Black Screen)    
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

    private void ShowDeskOverLay()
    {
        Debug.Log("ShowDeskOverlay called");
        deskOverlayImage.gameObject.SetActive(true);   
    }
    private void HideDeskOverLay()
    {
        Debug.Log("HideDeskOverlay called");
        deskOverlayImage.gameObject.SetActive(false);   
    }
}
