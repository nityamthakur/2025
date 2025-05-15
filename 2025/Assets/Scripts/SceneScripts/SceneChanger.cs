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
    [SerializeField] private DayEndScene dayEndScene;
    [SerializeField] private GameObject fadingScreenPrefab;
    private GameObject fadingScreen;
    public GameObject FadingScreen
    {
        get { return fadingScreen; }
        private set { fadingScreen = value; }
    }

    private Image fadingImage, lightsOutImage;
    private Image deskOverlayImage;
    private Button menuButton;

    private bool mainMenuDone = false;
    private int currentSceneIndex = 0;
    private List<Action> sceneSequence;

    public void Initialize()
    {
        if(gameManager == null)
        {
            Debug.Log("gameManager is null in SceneChanger");
        }
        // Define the order of the scenes
        sceneSequence = new List<Action>
        {
            () => dayStartScene.LoadDayStart(),
            () => jobScene.LoadJobStart(),
            () => dayEndScene.LoadDayEnd()
        };
        fadingScreen = Instantiate(fadingScreenPrefab);
        if (fadingScreen == null)
        {
            Debug.LogError("fadingImage is null in SceneManager.");
            return;
        }
        Canvas prefabCanvas = fadingScreen.GetComponentInChildren<Canvas>();
        if (prefabCanvas != null)
        {
            prefabCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            prefabCanvas.worldCamera = Camera.main;
        }


        fadingImage = fadingScreen.transform.Find("FadingImage").GetComponent<Image>();
        if (fadingImage == null)
        {
            Debug.LogError("Failed to find FadingImage component in SceneManager");
            return;
        }

        lightsOutImage = fadingScreen.transform.Find("LightsOutImage").GetComponent<Image>();
        if (lightsOutImage == null)
        {
            Debug.LogError("Failed to find LightsOutImage component in SceneManager");
            return;
        }
        lightsOutImage.gameObject.SetActive(false);

        // Used for ensuring the media enters and leaves behind certain screen elements
        deskOverlayImage = fadingScreen.transform.Find("DeskOverlay").GetComponent<Image>();
        if (deskOverlayImage == null)
        {
            Debug.LogError("Failed to find DeskOverlay component in SceneManager");
            return;
        }
        deskOverlayImage.gameObject.SetActive(false);

        menuButton = fadingScreen.transform.Find("MenuButton").GetComponent<Button>();
        if (menuButton == null)
        {
            Debug.LogError("Failed to find menuButton component in SceneManager");
            return;
        }
        menuButton.onClick.AddListener(() =>
        {
            EventManager.OpenOptionsMenu?.Invoke();
            EventManager.OptionsChanger?.Invoke(""); 
            EventManager.DisplayMenuButton?.Invoke(false);
            EventManager.PlaySound?.Invoke("switch1", true);
        });
        menuButton.gameObject.SetActive(false);

        dayStartScene.Initialize();
        jobScene.Initialize();
        dayEndScene.Initialize();
    }

    public void StartGame(int loadSlot)
    {
        if(loadSlot > 0)
        {
            PlayerPrefs.SetInt("LoadSlot", -1);
            EventManager.NextScene?.Invoke();

            // Continue Playtime counter
            StartCoroutine(gameManager.UpdatePlayTime());
        }
        else if( PlayerPrefs.GetInt("GameDevLoadDay") > 0)
        {
            gameManager.gameData.day = PlayerPrefs.GetInt("GameDevLoadDay");
            PlayerPrefs.SetInt("GameDevLoadDay", -1);
            EventManager.NextScene?.Invoke();
        }
        else
        {
            PlayerPrefs.SetInt("LoadSlot", -1);

            // Start Game
            // Comment out if using with debug
            //mainMenuScene.LoadMainMenu();                                     uncomment

            // For Debugging
            // Change the starting day
            
            // Start the game at day end
            //currentSceneIndex = 3;
            //dayEndScene.LoadDayEnd();
            
            // Start the game at the job scene
        }
    }

    public void StartNextScene()
    {
        if(!mainMenuDone)
        {
            // Start Playtime counter for first time
            StartCoroutine(gameManager.UpdatePlayTime());
        }

        mainMenuDone = true;
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
        EventManager.ShowLightsOutImage += ShowLightsOutImage;
        EventManager.HideLightsOutImage += HideLightsOutImage;
        EventManager.DisplayMenuButton += DisplayMenuButton;
    }

    void OnDisable()
    {
        EventManager.NextScene -= StartNextScene;
        EventManager.FadeIn -= FadeInScreen;
        EventManager.FadeOut -= FadeOutScreen;
        EventManager.ShowDeskOverlay -= ShowDeskOverLay;
        EventManager.HideDeskOverlay -= HideDeskOverLay;
        EventManager.ShowLightsOutImage += ShowLightsOutImage;
        EventManager.HideLightsOutImage += HideLightsOutImage;
        EventManager.DisplayMenuButton -= DisplayMenuButton;
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
        //Debug.Log("ShowDeskOverlay called");
        deskOverlayImage.gameObject.SetActive(true);   
    }
    private void HideDeskOverLay()
    {
        //Debug.Log("HideDeskOverlay called");
        deskOverlayImage.gameObject.SetActive(false);   
    }

    private void ShowLightsOutImage()
    {
        //Debug.Log("ShowDeskOverlay called");
        if(lightsOutImage != null)
            lightsOutImage.gameObject.SetActive(true);   
    }
    private void HideLightsOutImage()
    {
        //Debug.Log("HideDeskOverlay called");
        if(lightsOutImage != null)
            lightsOutImage.gameObject.SetActive(false);   
    }

    private void DisplayMenuButton(bool active)
    {
        if(mainMenuDone)
        {
            menuButton.gameObject.SetActive(active);
        }
    }
}
