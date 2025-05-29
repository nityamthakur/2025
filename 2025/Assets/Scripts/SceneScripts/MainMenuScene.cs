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
    private Button playButton, loadButton, optionsButton, creditsButton, exitButton;
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
        backgroundImage = FindObject<Image>("BackgroundImage");
        if (mainMenuImage.Length > 0)
        {
            animationCoroutine = StartCoroutine(CycleBackgroundFrames());
        }

        playButton = FindObject<Button>("PlayButton");
        playButton.onClick.AddListener(() =>
        {
            playButton.interactable = false;
            EventManager.PlaySound?.Invoke("switch1", true); 
            EventManager.OpenOptionsMenu?.Invoke();
            EventManager.NewStartGame?.Invoke(); 
            EventManager.PlaySound?.Invoke("switch1", true);
        });

        loadButton = FindObject<Button>("LoadButton");
        loadButton.onClick.AddListener(() =>
        {
            loadButton.interactable = false;
            EventManager.OpenOptionsMenu?.Invoke();
            EventManager.OptionsChanger?.Invoke("load"); 
            EventManager.PlaySound?.Invoke("switch1", true); 
        });

        optionsButton = FindObject<Button>("OptionsButton");
        optionsButton.onClick.AddListener(() =>
        {
            optionsButton.interactable = false;
            EventManager.OpenOptionsMenu?.Invoke();
            EventManager.OptionsChanger?.Invoke("options"); 
            EventManager.PlaySound?.Invoke("switch1", true); 
        });

        creditsButton = FindObject<Button>("CreditsButton");
        creditsButton.onClick.AddListener(() =>
        {
        });

        exitButton = FindObject<Button>("ExitButton");
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

    private T FindObject<T>(string name) where T : Component
    {
        return FindComponentByName<T>(name);
    }

    private T FindComponentByName<T>(string name) where T : Component
    {
        T[] components = currentMenuObject.GetComponentsInChildren<T>(true); // Search all children, even inactive ones

        foreach (T component in components)
        {
            if (component.gameObject.name == name)
                return component;
        }

        Debug.LogWarning($"Component '{name}' not found!");
        return null;
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
        if (playButton != null && playButton.IsActive())
        {
            playButton.interactable = true;
            loadButton.interactable = true;
            optionsButton.interactable = true;
            creditsButton.interactable = true;
            exitButton.interactable = true;
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
