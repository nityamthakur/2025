using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuObject;
    [SerializeField] private Sprite mainMenuImage;
    private GameObject currentMenuObject;
    private Button playButton;
    private Button loadButton;
    private Button optionsButton;
    private Image backgroundImage;

    public void LoadMainMenu() {
        
        currentMenuObject = Instantiate(menuObject);
        if (menuObject == null)
        {
            Debug.LogError("menuObject is null.");
            return;
        }

        SetUpMainMenu();
        EventManager.FadeIn?.Invoke(); 


        //Debug.Log("Forcing PlayButton Click AFTER Setup inside of LoadMainMenu"); 
        playButton.onClick.Invoke();
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
        playButton.onClick.AddListener(StartGame);

        loadButton = currentMenuObject.transform.Find("LoadButton").GetComponent<Button>();
        if (loadButton == null)
        {
            Debug.LogError("Failed to find loadButton component in MainMenu.");
            return;
        }
        loadButton.onClick.AddListener(StartGame);

        optionsButton = currentMenuObject.transform.Find("OptionsButton").GetComponent<Button>();
        if (optionsButton == null)
        {
            Debug.LogError("Failed to find optionsButton component in MainMenu.");
            return;
        }
        optionsButton.onClick.AddListener(StartGame);
    }

    public void StartGame(){
        Debug.Log("Game starting");

        EventManager.FadeOut?.Invoke();  

        Destroy(currentMenuObject);
        currentMenuObject = null;
        
        EventManager.NextScene?.Invoke();    
    }

    private void LoadGame(){
        // Will set up after load screen and saving finished
    }

    private void OptionsMenu(){
        // Will set up after options screen and volume, grayscale, anything else is finished
    }

}
