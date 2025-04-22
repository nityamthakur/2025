using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    private AudioManager audioManager;
    private SubtitleManager subtitleManager;
    private List<GameObject> sections = new();
    private GameObject pauseMenu, menuSections, audioSection, saveLoadSection, confirmSection;
    private TextMeshProUGUI confirmText, pauseMenuText;
    private Action confirmAction, cancelAction;
    private Button slot1Button, slot2Button, slot3Button, backButton, saveButton, deleteButton, mainMenuButton;
    private Slider masterVolumeSlider, musicVolumeSlider, sfxVolumeSlider;
    private Toggle muteToggle, subtitleToggle, grayscaleToggle, fullScreenToggle;
    private GameManager gameManager;
    private bool isLoadingSettings = false, deleteButtonPressed = false;
    private string lastSaveLoadOption = "";

    private void Awake()
    {
        FindGameManager();
        
        SetUpSections();
        SetupMenuButtons();
        ScreenModeSetUp();
        GrayscaleSetUp();
        AudioSetUp();
        
        LoadPersistentSettings();

        // Allow sounds to be played after everything intiallized to 
        // prevent button trigger sounds before game start
        audioManager.canPlaySounds = true;
        
        // Hide the menu after initialization
        gameObject.SetActive(false);
    }

    private void SetUpSections()
    {
        pauseMenu = transform.Find("PauseMenuBackground")?.gameObject;
        menuSections = pauseMenu.transform.Find("MenuSections")?.gameObject;
        audioSection = pauseMenu.transform.Find("AudioMenu")?.gameObject;
        saveLoadSection = pauseMenu.transform.Find("SaveLoadMenu")?.gameObject;
        confirmSection = pauseMenu.transform.Find("ConfirmMenu")?.gameObject;

        if (pauseMenu == null) 
            Debug.Log("Couldnt find PauseMenuBackground");
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
    }

    private void SetupMenuButtons()
    {
        confirmText = FindComponentByName<TextMeshProUGUI>("ConfirmText");
        pauseMenuText = FindComponentByName<TextMeshProUGUI>("PauseMenuText");

        FindButton("CloseMenuButton", () =>
        {
            EventManager.DisplayMenuButton?.Invoke(true);
            EventManager.ReactivateMainMenuButtons?.Invoke();
            EventManager.PlaySound?.Invoke("switch1");
            Time.timeScale = 1;
            gameObject.SetActive(false);
            deleteButtonPressed = false;
        });
      
        backButton = FindButton("BackButton", () =>
        {
            deleteButtonPressed = false;
            ChangeMenuSection(menuSections);
            EventManager.PlaySound?.Invoke("switch1");
        });
        backButton?.gameObject.SetActive(false);

        saveButton = FindButton("SaveButton", () =>
        {
            lastSaveLoadOption = "save";
            ChangeMenuSection(saveLoadSection);
            UpdateSaveLoadButtons("save");
            EventManager.PlaySound?.Invoke("switch1"); 
        });
        // Hide the save button initally in the main menu
        saveButton?.gameObject.SetActive(false);

        FindButton("LoadButton", () =>
        {
            lastSaveLoadOption = "load";
            ChangeMenuSection(saveLoadSection);
            UpdateSaveLoadButtons("load");
            EventManager.PlaySound?.Invoke("switch1"); 
        });

        deleteButton = FindButton("DeleteButton", () =>
        {
            deleteButtonPressed = true;
            UpdateSaveLoadButtons("delete");
            ChangeMenuSection(saveLoadSection);
            EventManager.PlaySound?.Invoke("switch1"); 
        });
        deleteButton?.gameObject.SetActive(false);

        FindButton("OptionsButton", () =>
        {
            ChangeMenuSection(audioSection);
            EventManager.PlaySound?.Invoke("switch1"); 
        });

        mainMenuButton = FindButton("MainMenuButton", () =>
        {
            EventManager.PlaySound?.Invoke("switch1");
            ChangeObjectText(confirmText, "Return to \nMain Menu?");
            ChangeMenuSection(confirmSection);

            // Store the action to execute if "Yes" is clicked
            confirmAction = () =>
            {
                gameManager.RestartGame();
            };
            cancelAction = () =>
            {
                ChangeMenuSection(menuSections);
            };
        });
        // Hide the main menu button initally in the main menu
        mainMenuButton?.gameObject.SetActive(false);

        FindButton("QuitGameButton", () =>
        {
            EventManager.PlaySound?.Invoke("switch1");
            ChangeObjectText(confirmText, "Close the Game?");
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

        FindButton("YesButton", () =>
        {
            EventManager.PlaySound?.Invoke("switch1"); 
            confirmAction?.Invoke();
        });

        FindButton("NoButton", () =>
        {
            EventManager.PlaySound?.Invoke("switch1"); 
            cancelAction?.Invoke();
        });

        // Save and Load Slot Buttons
        slot1Button = FindButton("Slot1Button");
        slot2Button = FindButton("Slot2Button");
        slot3Button = FindButton("Slot3Button");
        UpdateSlotInformation();

    }

    private Button FindButton(string name, Action action = null)
    {
        Button button = FindComponentByName<Button>(name);
        if (button != null && action != null)
        {
            //button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action.Invoke());
        }
        return button;
    }

    // Swaps game save slot buttons between, saving, loading, and deleting.
    private void UpdateSaveLoadButtons(string saveButtonPressed)
    {
        Button[] slots = { slot1Button, slot2Button, slot3Button };
        for (int i = 0; i < slots.Length; i++)
        {
            int slotIndex = i + 1;
            slots[i].onClick.RemoveAllListeners();
            slots[i].onClick.AddListener(() =>
            {
                EventManager.PlaySound?.Invoke("switch1");
                if(saveButtonPressed == "save")
                    HandleSaveSlot(slotIndex);
                else if(saveButtonPressed == "load")
                    HandleLoadSlot(slotIndex);
                else    
                    HandleDeleteSlot(slotIndex);
            });
        }
    }

    private void HandleSaveSlot(int slot)
    {
        if(SaveSystem.SaveExists(slot))
        {
            ChangeObjectText(confirmText, $"Save Over File {slot}?");
            ChangeMenuSection(confirmSection);

            confirmAction = () =>
            {
                SaveSystem.SaveGame(slot, gameManager.gameData);
                ChangeMenuSection(saveLoadSection);
            };
            cancelAction = () =>
            {
                ChangeMenuSection(saveLoadSection);
            };
        }
        else
        {
            SaveSystem.SaveGame(slot, gameManager.gameData);
            ChangeMenuSection(saveLoadSection);
        }
    }

    private void HandleLoadSlot(int slot)
    {
        GameObject gameManagerObject = GameObject.Find("GameManager");
        GameManager gameManager = gameManagerObject.GetComponent<GameManager>();
        if (gameManagerObject == null || gameManager == null)
        {
            Debug.LogError("GameManager component or gamemanagerObject not found in OptionsMenu!");
            return;
        }

        if(SaveSystem.SaveExists(slot))
        {
            ChangeObjectText(confirmText, $"Load Game File {slot}?");
            ChangeMenuSection(confirmSection);

            confirmAction = () =>
            {
                PlayerPrefs.SetInt("LoadSlot", slot);
                Time.timeScale = 1;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            };
            cancelAction = () =>
            {
                ChangeMenuSection(saveLoadSection);
            };
        }
    }

    private void HandleDeleteSlot(int slot)
    {
        GameObject gameManagerObject = GameObject.Find("GameManager");
        GameManager gameManager = gameManagerObject.GetComponent<GameManager>();
        if (gameManagerObject == null || gameManager == null)
        {
            Debug.LogError("GameManager component or gamemanagerObject not found in OptionsMenu!");
            return;
        }

        if(SaveSystem.SaveExists(slot))
        {
            ChangeObjectText(confirmText, $"Delete Game File {slot}?");
            ChangeObjectText(pauseMenuText, "Select a save file to delete");
            ChangeMenuSection(confirmSection);

            confirmAction = () =>
            {
                deleteButtonPressed = false;
                SaveSystem.DeleteSave(slot);
                ChangeMenuSection(saveLoadSection);
                UpdateSaveLoadButtons(lastSaveLoadOption);
            };
            cancelAction = () =>
            {
                deleteButtonPressed = false;
                ChangeMenuSection(saveLoadSection);
                UpdateSaveLoadButtons(lastSaveLoadOption);
            };
        }
        else
        {
            deleteButtonPressed = false;
        }      
    }

    private void UpdateSlotInformation()
    {
        if(slot1Button == null || slot2Button == null || slot3Button == null)
        {
            Debug.Log("One or more Save/Load button slots are null in Options Menu");
            return;
        }        

        Button[] slots = { slot1Button, slot2Button, slot3Button };
        for (int i = 0; i < slots.Length; i++)
        {
            int slotIndex = i + 1;

            // Find the TextMeshProUGUI inside the button
            TextMeshProUGUI buttonText = slots[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText == null)
            {
                Debug.LogWarning($"No TextMeshProUGUI found in Slot {slotIndex} button!");
                return;
            }

            if(SaveSystem.SaveExists(slotIndex))
            {
                GameData gameSlot = SaveSystem.LoadGame(slotIndex);
                if (gameSlot != null)
                {
                    // Convert playtime to "H:MM:SS" format
                    string time = SecondMinuteHourConversion(gameSlot.playTime);
                    buttonText.text = $"Save Slot {slotIndex}\nPlaytime: {time}  Day: {gameSlot.day}";

                    //Debug.Log($"The gameSlot{slotIndex} playTime is: {gameSlot.playTime}");
                    //Debug.Log($"buttonText is now: {buttonText.text}");
                }
                else
                {
                    Debug.Log($"Failed to get Game Slot {slotIndex}");
                }
            }
            else
                buttonText.text = $"Save Slot {slotIndex}";
        }
    }

    // Converts seconds into Hours:Minutes
    private string SecondMinuteHourConversion(float seconds)
    {
        int hours = (int)(seconds / 3600);
        int minutes = (int)((seconds % 3600) / 60);
        int remainingSeconds = (int)(seconds % 60);

        return $"{hours}:{minutes:D2}:{remainingSeconds:D2}";
    }


    private void ChangeMenuSection(GameObject section)
    {
        UpdateSlotInformation();

        if(section == audioSection)
            ChangeObjectText(pauseMenuText, "Options Menu");
        else if(section == saveLoadSection || section == confirmSection)
        {
            if(deleteButtonPressed)
                ChangeObjectText(pauseMenuText, "Select a save file to delete");
            else
            {
                if(lastSaveLoadOption == "save")
                    ChangeObjectText(pauseMenuText, "Save Menu");
                else
                    ChangeObjectText(pauseMenuText, "Load Menu");
            }
        }
        else
            ChangeObjectText(pauseMenuText, "Pause Menu");


        if (section == menuSections)
            backButton.gameObject.SetActive(false);
        else
            backButton.gameObject.SetActive(true);

        if (section == saveLoadSection)
            deleteButton.gameObject.SetActive(true);
        else
            deleteButton.gameObject.SetActive(false);

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

    private void FindGameManager()
    {
        GameObject gameManagerObject = GameObject.Find("GameManager");
        if (gameManagerObject == null)
        {
            Debug.LogError("GameManager GameObject not found!");
            return;
        }
        
        gameManager = gameManagerObject.GetComponent<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager component not found!");
        }        
    }

    private void ChangeObjectText(TextMeshProUGUI gameObject, string text)
    {
        gameObject.text = text;
    }

    private void OptionsChanger(string option)
    {
        switch (option.ToLower())
        {
            case "load":
                UpdateSaveLoadButtons("load");
                ChangeMenuSection(saveLoadSection);
                break;

            case "options":
                ChangeMenuSection(audioSection);
                break;
            
            default:
                saveButton?.gameObject.SetActive(true);
                mainMenuButton?.gameObject.SetActive(true);
                ChangeMenuSection(menuSections);
                break;
        }
    }

    private void GrayscaleSetUp()
    {
        grayscaleToggle = FindComponentByName<Toggle>("GrayscaleToggle");
        TMP_Text grayscaleActiveText = FindComponentByName<TMP_Text>("GrayscaleOnOffText");

        if (grayscaleToggle != null && grayscaleActiveText != null)
        {
            // Set the initial state to match the current grayscale setting in Eventmanager
            grayscaleToggle.isOn = EventManager.IsGrayscale;
            grayscaleActiveText.text = EventManager.IsGrayscale ? "On" : "Off";

            // Listen for changes when toggle is clicked
            grayscaleToggle.onValueChanged.AddListener((bool isOn) =>
            {
                EventManager.PlaySound?.Invoke("switch1"); 
                EventManager.ToggleGrayscaleState();
                grayscaleActiveText.text = isOn ? "On" : "Off";
                SavePersistentSettings();
            });
        }
    }

    private void ScreenModeSetUp()
    {
        fullScreenToggle = FindComponentByName<Toggle>("FullScreenToggle");
        TMP_Text fullScreenActiveText = FindComponentByName<TMP_Text>("FullScreenOnOffText");

        if (fullScreenToggle != null && fullScreenActiveText != null)
        {
            // Set the initial state to match the current grayscale setting in Eventmanager
            fullScreenToggle.isOn = EventManager.IsFullScreen;
            fullScreenActiveText.text = EventManager.IsFullScreen ? "On" : "Off";

            // Listen for changes when toggle is clicked
            fullScreenToggle.onValueChanged.AddListener((bool isOn) =>
            {
                EventManager.PlaySound?.Invoke("switch1"); 
                EventManager.ToggleFullScreenState();
                fullScreenActiveText.text = isOn ? "On" : "Off";
                Screen.fullScreen = fullScreenToggle.isOn;
                SavePersistentSettings();
            });
        }
    }

    private void AudioSetUp()
    {
        // Locate AudioManager Object
        audioManager = FindFirstObjectByType<AudioManager>();
        if (audioManager == null)
        {
            Debug.Log("AudioManager not found in scene!");
            return;
        }

        // Locate SubtitleManager Object
        subtitleManager = FindFirstObjectByType<SubtitleManager>();
        if (subtitleManager == null)
        {
            Debug.Log("SubTitleManager not found in scene!");
            return;
        }

        // Sound Sliders
        masterVolumeSlider = FindComponentByName<Slider>("MasterVolume");
        if(masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(value => {
                audioManager.UpdateMasterVolume(value);
                SavePersistentSettings();
            });
        }

        musicVolumeSlider = FindComponentByName<Slider>("MusicVolume");
        if(musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(value => {
                audioManager.UpdateMusicVolume(value);
                SavePersistentSettings();
            });
        }

        sfxVolumeSlider = FindComponentByName<Slider>("SFXVolume");
        if(sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(value => {
                audioManager.UpdateSFXVolume(value);
                SavePersistentSettings();
            });    
        }


        // Sound Toggle
        muteToggle = FindComponentByName<Toggle>("MuteToggle");
        TMP_Text muteActiveText = FindComponentByName<TMP_Text>("MuteOnOffText");
        if (muteToggle != null && muteActiveText != null)
        {
            // Listen for changes when toggle is clicked
            muteToggle.onValueChanged.AddListener((bool isOn) =>
            {
                audioManager.MuteToggle(isOn);
                muteActiveText.text = isOn ? "On" : "Off";
                EventManager.PlaySound?.Invoke("switch1"); 
                SavePersistentSettings();
            });
        }

        // Subtitle Toggle
        subtitleToggle = FindComponentByName<Toggle>("SubtitleToggle");
        TMP_Text subtitleActiveText = FindComponentByName<TMP_Text>("SubtitleOnOffText");
        if (subtitleToggle != null && subtitleActiveText != null)
        {
            // Listen for changes when toggle is clicked
            subtitleToggle.onValueChanged.AddListener((bool isOn) =>
            {
                subtitleManager.SubtitleToggle(isOn);
                subtitleActiveText.text = isOn ? "On" : "Off";
                EventManager.PlaySound?.Invoke("switch1"); 
                SavePersistentSettings();
            });
        }
    }

    public void SavePersistentSettings()
    {
        if (isLoadingSettings) return; // 🔹 Prevent saving while loading settings

        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        PlayerPrefs.SetInt("MuteState", muteToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("SubtitleState", subtitleToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("GrayState", grayscaleToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("FullScreenState", fullScreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();  // Write to disk immediately
    }

    public void LoadPersistentSettings()
    {
        isLoadingSettings = true;  // Prevent SavePersistentSettings from being called

        float master = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1.0f);

        masterVolumeSlider.value = master;
        musicVolumeSlider.value = music;
        sfxVolumeSlider.value = sfx;

        audioManager.UpdateMasterVolume(master);
        audioManager.UpdateMusicVolume(music);
        audioManager.UpdateSFXVolume(sfx);

        int muteOn = PlayerPrefs.GetInt("MuteState", 0);
        muteToggle.isOn = muteOn == 1;
        audioManager.MuteToggle(muteToggle.isOn);

        int subtitleOn = PlayerPrefs.GetInt("SubtitleState", 0);
        subtitleToggle.isOn = subtitleOn == 1;
        subtitleManager.SubtitleToggle(subtitleToggle.isOn);

        int grayOn = PlayerPrefs.GetInt("GrayState", 0);
        grayscaleToggle.isOn = grayOn == 1;

        int fullScreenOn = PlayerPrefs.GetInt("FullScreenState", 0);
        fullScreenToggle.isOn = fullScreenOn == 1;
        Screen.fullScreen = fullScreenToggle.isOn;
        
        isLoadingSettings = false;
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
