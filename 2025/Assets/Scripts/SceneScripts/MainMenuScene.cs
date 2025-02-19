using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
            playButton.interactable = false; // Disable immediately
            StartCoroutine(StartGame());
        });

        loadButton = currentMenuObject.transform.Find("LoadButton").GetComponent<Button>();
        if (loadButton == null)
        {
            Debug.LogError("Failed to find loadButton component in MainMenu.");
            return;
        }
        loadButton.onClick.AddListener(() =>
        {
            loadButton.interactable = false; // Disable immediately
            LoadGame();
        });

        optionsButton = currentMenuObject.transform.Find("OptionsButton").GetComponent<Button>();
        if (optionsButton == null)
        {
            Debug.LogError("Failed to find optionsButton component in MainMenu.");
            return;
        }
        optionsButton.onClick.AddListener(() =>
        {
            optionsButton.interactable = false; // Disable immediately
            OptionsMenu();
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
        Debug.Log("Load Button pressed");
    }

    private void OptionsMenu(){
        // Will set up after options screen and volume, grayscale, anything else is finished
        Debug.Log("Options Button pressed");
    }

}
