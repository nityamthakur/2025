using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class MainMenuScene : MonoBehaviour
{
    [SerializeField] private GameObject menuObject;
    [SerializeField] private Sprite mainMenuImage;
    private GameObject currentMenuObject;
    private Button playButton;
    private Button loadButton;
    private Button optionsButton;
    private Image backgroundImage;

    public void LoadMainMenu() {
        EventManager.PlayMusic?.Invoke("menu"); 

        currentMenuObject = Instantiate(menuObject);
        if (menuObject == null)
        {
            Debug.LogError("menuObject is null.");
            return;
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
        backgroundImage.sprite = mainMenuImage; 

        playButton = currentMenuObject.transform.Find("PlayButton").GetComponent<Button>();
        if (playButton == null)
        {
            Debug.LogError("Failed to find playButton component in MainMenu.");
            return;
        }
        playButton.onClick.AddListener(() =>
        {
            playButton.interactable = false;
            StartCoroutine(StartGame());
            EventManager.StopMusic?.Invoke(); 
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
            LoadGame();
            EventManager.PlaySound?.Invoke("switch1"); 
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
            OptionsMenu();
            EventManager.PlaySound?.Invoke("switch1"); 
        });
    
    }
    private IEnumerator StartGame()
    {
        EventManager.FadeOut?.Invoke();
        yield return new WaitForSeconds(2f);

        Destroy(currentMenuObject);
        currentMenuObject = null;

        yield return new WaitForSeconds(2f);
        EventManager.NextScene?.Invoke();
    }

    private void LoadGame(){
        // Will set up after load screen and saving finished
        EventManager.OpenOptionsMenu?.Invoke();
    }

    private void OptionsMenu(){
        // Will set up after options screen and volume, grayscale, anything else is finished
        EventManager.OpenOptionsMenu?.Invoke();
    }

    private void ReactivateMainMenuButtons()
    {
        // Prevent reactivating load and open buttons when game has started
        if(playButton.IsActive())
        {
            loadButton.interactable = true;
            optionsButton.interactable = true;
        }
    }


    void OnEnable()
    {
        EventManager.ReactivateMainMenuButtons += ReactivateMainMenuButtons;
    }

    void OnDisable()
    {
        EventManager.ReactivateMainMenuButtons -= ReactivateMainMenuButtons;
    }
}
