using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    private AudioManager audioManager;
    private List<GameObject> sections = new();
    private GameObject menuSections;
    private GameObject audioSection;
    private GameObject saveLoadSection;
    private GameObject confirmSection;
    private TextMeshProUGUI confirmText;
    private System.Action confirmAction;
    private System.Action cancelAction;

    private Button backButton;
    //private bool saveSelected = true; // For differentiating between saving and loading

    public void AddAudioComponent(AudioManager audioObject)
    {
        audioManager = audioObject;
    }

    private void Start()
    {
        menuSections = transform.Find("MenuSections")?.gameObject;
        audioSection = transform.Find("AudioMenu")?.gameObject;
        saveLoadSection = transform.Find("SaveLoadMenu")?.gameObject;
        confirmSection = transform.Find("ConfirmMenu")?.gameObject;

        if (menuSections != null) 
            sections.Add(menuSections);
        if (audioSection != null) 
            sections.Add(audioSection);
        if (saveLoadSection != null) 
            sections.Add(saveLoadSection);
        if (confirmSection != null) 
            sections.Add(confirmSection);

        // Hide all sections initially
        foreach (GameObject section in sections)
        {
            section.SetActive(false);
        }

        SetUpMenuSections();
        
        // Close button setup
        Button closeButton = FindComponentByName<Button>("CloseMenuButton");
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() =>
            {
                EventManager.DisplayMenuButton?.Invoke(true); 
                EventManager.ReactivateMainMenuButtons?.Invoke(); 
                EventManager.PlaySound?.Invoke("switch1"); 
                Time.timeScale = 1;
                this.gameObject.SetActive(false);
            });
        }        

        // Close button setup
        backButton = FindComponentByName<Button>("BackButton");
        if (backButton != null)
        {
            backButton.onClick.AddListener(() =>
            {
                ChangeMenuSection(menuSections);
                EventManager.PlaySound?.Invoke("switch1"); 
            });
            backButton.gameObject.SetActive(false);
        }
        else
            Debug.Log("backButton is null in Options Menu");
        
        GrayscaleSetUp();
        AudioSetUp();

        // Hide the menu after 
        transform.gameObject.SetActive(false);
    }

    private void SetUpMenuSections()
    {
        Button saveButton = FindComponentByName<Button>("SaveButton");
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(() =>
            {
                ChangeMenuSection(saveLoadSection);
                EventManager.PlaySound?.Invoke("switch1"); 
            });
        }

        Button loadButton = FindComponentByName<Button>("LoadButton");
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(() =>
            {
                ChangeMenuSection(saveLoadSection);
                EventManager.PlaySound?.Invoke("switch1"); 
            });
        }

        Button optionsButton = FindComponentByName<Button>("OptionsButton");
        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(() =>
            {
                ChangeMenuSection(audioSection);
                EventManager.PlaySound?.Invoke("switch1"); 
            });
        }

        // TODO: Confirmation Screen Section for these two
        Button mainButton = FindComponentByName<Button>("MainMenuButton");
        if (mainButton != null)
        {
            mainButton.onClick.AddListener(() =>
            {
                EventManager.PlaySound?.Invoke("switch1");
                confirmText.text = "Return to \nMain Menu?";
                ChangeMenuSection(confirmSection);

                // Store the action to execute if "Yes" is clicked
                confirmAction = () =>
                {
                    Debug.Log("Restarting Game");
                    Time.timeScale = 1;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                };
                cancelAction = () =>
                {
                    ChangeMenuSection(menuSections);
                };
            });
        }


        Button quitButton = FindComponentByName<Button>("QuitGameButton");
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() =>
            {
                EventManager.PlaySound?.Invoke("switch1");
                confirmText.text = "Close the Game?";
                ChangeMenuSection(confirmSection);

                // Store the action to execute if "Yes" is clicked
                confirmAction = () =>
                {
                    Application.Quit(); // For standalone builds

                    #if UNITY_EDITOR
                    UnityEditor.EditorApplication.ExitPlaymode(); // For Unity Editor testing
                    #endif
                };
                cancelAction = () =>
                {
                    ChangeMenuSection(menuSections);
                };
            });
        }

        Button yesButton = FindComponentByName<Button>("YesButton");
        if (yesButton != null)
        {
            yesButton.onClick.AddListener(() =>
            {
                EventManager.PlaySound?.Invoke("switch1"); 
                confirmAction?.Invoke();
            });
        }

        Button noButton = FindComponentByName<Button>("NoButton");
        if (noButton != null)
        {
            noButton.onClick.AddListener(() =>
            {
                EventManager.PlaySound?.Invoke("switch1"); 
                cancelAction?.Invoke();
            });
        }

        confirmText = FindComponentByName<TextMeshProUGUI>("ConfirmText");
    }

    private void ChangeMenuSection(GameObject section)
    {
        if (section == menuSections)
            backButton.gameObject.SetActive(false);
        else
            backButton.gameObject.SetActive(true);

        foreach (GameObject data in sections)
        {
            if (data == section)
            {
                data.SetActive(false); // Force UI refresh by toggling off first
                data.SetActive(true);
            }
            else
            {
                data.SetActive(false);
            }
        }
    }

    private void OptionsChanger(string option)
    {
        switch (option.ToLower())
        {
            case "load":
                ChangeMenuSection(saveLoadSection);
                break;

            case "options":
                ChangeMenuSection(audioSection);
                break;
            
            default:
                ChangeMenuSection(audioSection);
                break;
        }
    }

    private void GrayscaleSetUp()
    {
        Toggle grayScaleToggle = FindComponentByName<Toggle>("GrayscaleToggle");
        TMP_Text grayScaleActiveText = FindComponentByName<TMP_Text>("GrayscaleOnOffText");

        if (grayScaleToggle != null && grayScaleActiveText != null)
        {
            // Set the initial state to match the current grayscale setting in Eventmanager
            // Will eventually set itself via game load and pass to Eventmanager
            grayScaleToggle.isOn = EventManager.IsGrayscale;
            grayScaleActiveText.text = EventManager.IsGrayscale ? "On" : "Off";

            // Listen for changes when toggle is clicked
            grayScaleToggle.onValueChanged.AddListener((bool isOn) =>
            {
                EventManager.PlaySound?.Invoke("switch1"); 
                EventManager.ToggleGrayscaleState();
                grayScaleActiveText.text = isOn ? "On" : "Off";
            });
        }
    }

    private void AudioSetUp()
    {
        // Sound Sliders
        Slider masterVolumeSlider = FindComponentByName<Slider>("MasterVolume");
        if(masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(audioManager.UpdateMasterVolume);

        Slider musicVolumeSlider = FindComponentByName<Slider>("MusicVolume");
        if(musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(audioManager.UpdateMusicVolume);

        Slider sfxVolumeSlider = FindComponentByName<Slider>("SFXVolume");
        if(sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(audioManager.UpdateSFXVolume);

        // Sound Toggles
        Toggle muteToggle = FindComponentByName<Toggle>("MuteToggle");
        TMP_Text muteActiveText = FindComponentByName<TMP_Text>("MuteOnOffText");

        if (muteToggle != null && muteActiveText != null)
        {
            // Listen for changes when toggle is clicked
            muteToggle.onValueChanged.AddListener((bool isOn) =>
            {
                audioManager.MuteToggle(isOn);
                muteActiveText.text = isOn ? "On" : "Off";
                EventManager.PlaySound?.Invoke("switch1"); 
            });
        }

        // Subtitles not yet implemented
        Toggle subtitleToggle = FindComponentByName<Toggle>("SubtitleToggle");
        TMP_Text subtitleActiveText = FindComponentByName<TMP_Text>("SubtitleOnOffText");

        if (subtitleToggle != null && subtitleToggle != null)
        {
            // Listen for changes when toggle is clicked
            subtitleToggle.onValueChanged.AddListener((bool isOn) =>
            {
                subtitleActiveText.text = isOn ? "On" : "Off";
                Debug.Log("Subtitle Toggle pressed. Not yet implemented");
            });
        }
    }

    private T FindComponentByName<T>(string name) where T : Component
    {
        T[] components = GetComponentsInChildren<T>(true); // Search all children, even inactive ones
        
        foreach (T component in components)
        {
            if (component.gameObject.name == name)
                return component;
        }

        Debug.LogWarning($"Component '{name}' not found!");
        return null;
    }

    void OnEnable()
    {
        EventManager.OptionsChanger += OptionsChanger;
    }

    void OnDisable()
    {
        EventManager.OptionsChanger -= OptionsChanger;
    }
}
