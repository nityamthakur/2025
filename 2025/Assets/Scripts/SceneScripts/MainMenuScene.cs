using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class MainMenuScene : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject menuObject;
    [SerializeField] private Sprite[] mainMenuImage;
    [SerializeField] private float frameInterval = 0.5f; // Interval for frame cycling
    private GameObject currentMenuObject;
    private Button playButton;
    private Button loadButton;
    private Button optionsButton;
    private Button exitButton;

    private Image backgroundImage;
    private Coroutine animationCoroutine;

    public void LoadMainMenu() {
        EventManager.PlayMusic?.Invoke("menu"); 

        currentMenuObject = Instantiate(menuObject);
        if (menuObject == null)
        {
            Debug.LogError("menuObject is null.");
            return;
        }
        Canvas prefabCanvas = currentMenuObject.GetComponentInChildren<Canvas>();
        if (prefabCanvas != null)
        {
            prefabCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            prefabCanvas.worldCamera = Camera.main;
        }

        SetUpMainMenu();
        EventManager.FadeIn?.Invoke(); 
    }

    private void SetUpMainMenu()
    {
        backgroundImage = currentMenuObject.transform.Find("BackgroundImage").GetComponent<Image>();
        if (backgroundImage == null)
        {
            Debug.Log("Failed to find Image component in MainMenu");
            return;
        }

        if (mainMenuImage.Length > 0)
        {
            animationCoroutine = StartCoroutine(CycleBackgroundFrames());
        }

        playButton = currentMenuObject.transform.Find("PlayButton").GetComponent<Button>();
        if (playButton == null)
        {
            Debug.LogError("Failed to find playButton component in MainMenu.");
            return;
        }
        playButton.onClick.AddListener(() =>
        {
            playButton.interactable = false;
            EventManager.PlaySound?.Invoke("switch1", true); 
            EventManager.OpenOptionsMenu?.Invoke();
            EventManager.NewStartGame?.Invoke(); 
            EventManager.PlaySound?.Invoke("switch1", true);
        });

        loadButton = currentMenuObject.transform.Find("LoadButton").GetComponent<Button>();
        if (loadButton == null)
        {
            Debug.LogError("Failed to find loadButton component in MainMenu.");
            return;
        }
        loadButton.onClick.AddListener(() =>
        {
            loadButton.interactable = false;
            EventManager.OpenOptionsMenu?.Invoke();
            EventManager.OptionsChanger?.Invoke("load"); 
            EventManager.PlaySound?.Invoke("switch1", true); 
        });

        optionsButton = currentMenuObject.transform.Find("OptionsButton").GetComponent<Button>();
        if (optionsButton == null)
        {
            Debug.LogError("Failed to find optionsButton component in MainMenu.");
            return;
        }
        optionsButton.onClick.AddListener(() =>
        {
            optionsButton.interactable = false;
            EventManager.OpenOptionsMenu?.Invoke();
            EventManager.OptionsChanger?.Invoke("options"); 
            EventManager.PlaySound?.Invoke("switch1", true); 
        });

        exitButton = currentMenuObject.transform.Find("ExitButton").GetComponent<Button>();
        if (exitButton == null)
        {
            Debug.LogError("Failed to find exitButton component in MainMenu.");
            return;
        }
        exitButton.onClick.AddListener(() =>
        {
            optionsButton.interactable = false;
            Application.Quit(); // For standalone builds

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode(); // For Unity Editor testing
            #endif
        });
    }

    private IEnumerator CycleBackgroundFrames()
    {
        int frameIndex = 0;
        while (true)
        {
            backgroundImage.sprite = mainMenuImage[frameIndex];
            frameIndex = (frameIndex + 1) % mainMenuImage.Length;
            yield return new WaitForSeconds(frameInterval);
        }
    }

    private void BeginNewGame()
    {
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        EventManager.FadeOut?.Invoke();
        EventManager.StopMusic?.Invoke();
        yield return new WaitForSeconds(2f);

        StopCoroutine(animationCoroutine);
        Destroy(currentMenuObject);
        currentMenuObject = null;

        yield return new WaitForSeconds(2f);
        EventManager.NextScene?.Invoke();
    }

    private void ReactivateMainMenuButtons()
    {
        // Prevent reactivating load and open buttons when game has started
        if(playButton != null && playButton.IsActive())
        {
            playButton.interactable = true;
            loadButton.interactable = true;
            optionsButton.interactable = true;
        }
    }

    void OnEnable()
    {
        EventManager.ReactivateMainMenuButtons += ReactivateMainMenuButtons;
        EventManager.BeginNewGame += BeginNewGame;
    }

    void OnDisable()
    {
        EventManager.ReactivateMainMenuButtons -= ReactivateMainMenuButtons;
        EventManager.BeginNewGame -= BeginNewGame;
    }
}
