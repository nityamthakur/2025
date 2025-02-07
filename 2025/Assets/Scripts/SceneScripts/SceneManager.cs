using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    int day = 1;

    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private DayStart dayStart;
    [SerializeField] private GameObject fadingScreenPrefab;
    private GameObject fadingScreen;
    private Image fadingImage;
    //[SerializeField] private JobMode jobMode;
    //[SerializeField] private EndOfDay endOfDay;

    private int currentSceneIndex = 0;
    private List<Action> sceneSequence;

    void Awake()
    {
        // Define the order of the scenes
        sceneSequence = new List<Action>
        {
            () => dayStart.LoadDayStart(day),
            //() => jobMode.LoadJobStart(),
            //() => endOfDay.LoadEndOfDay()
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
    }

    void Start()
    {
        StartGameLoop();
    }

    public void StartGameLoop()
    {
        mainMenu.LoadMainMenu();
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
        EventManager.FadeIn += () => StartCoroutine(FadeIn());
        EventManager.FadeOut += () => StartCoroutine(FadeOut());
    }

    void OnDisable()
    {
        EventManager.NextScene -= StartNextScene;
        EventManager.FadeIn -= () => StartCoroutine(FadeIn());
        EventManager.FadeOut -= () => StartCoroutine(FadeOut());
    }

    private float fadeDuration = 2f; // Adjustable fade duration
    private float waitTime = 1f; // Time to wait before fading back in
    private IEnumerator FadeIn()
    {
        yield return StartCoroutine(FadeImage(fadingImage, 1f, 0f, fadeDuration)); // fadeInOut goes to 100% opacity (Black Screen) 
        fadingImage.gameObject.SetActive(false);
        Destroy(fadingScreen);
        fadingScreen = null;
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
}
