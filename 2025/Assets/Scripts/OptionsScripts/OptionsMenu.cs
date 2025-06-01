using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    private List<GameObject> sections = new();
    private GameObject pauseMenu, menuSections, audioSection, saveLoadSection, creditSection, confirmSection;
    private TextMeshProUGUI confirmText, pauseMenuText, backButtonText;
    private Action confirmAction, cancelAction;
    private Button slot1Button, slot2Button, slot3Button, backButton, saveButton, deleteButton, mainMenuButton;
    [SerializeField] private GameObject confirmButtons, difficultyButtons;
    private Slider masterVolumeSlider, musicVolumeSlider, sfxVolumeSlider, textSpeedSlider;
    private Toggle muteToggle, subtitleToggle, grayscaleToggle, fullScreenToggle;
    private GameManager gameManager;
    private AudioManager audioManager;
    private SceneChanger sceneChanger;
    private bool isLoadingSettings = false, deleteButtonPressed = false;
    private string lastSaveLoadOption = "";
    private GameObject currentSection;
    private void Awake()
    {
        FindManagers();
        
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

    // Loading Functions -------------------------------------------//
    // -------------------------------------------------------------//
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
    
    private T FindObject<T>(string name, Action action = null) where T : Component
    {
        T objectType = FindComponentByName<T>(name);

        if (typeof(T) == typeof(Button) && objectType != null && action != null)
        {
            Button button = objectType as Button;
            button.onClick.AddListener(() => action.Invoke());
        }

        return objectType;
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

    private void SetUpSections()
    {
        pauseMenu = transform.Find("PauseMenuBackground")?.gameObject;
        menuSections = pauseMenu.transform.Find("MenuSections")?.gameObject;
        audioSection = pauseMenu.transform.Find("AudioMenu")?.gameObject;
        saveLoadSection = pauseMenu.transform.Find("SaveLoadMenu")?.gameObject;
        creditSection = pauseMenu.transform.Find("CreditSection")?.gameObject;
        confirmSection = pauseMenu.transform.Find("ConfirmMenu")?.gameObject;

        if (pauseMenu == null)
            Debug.Log("Couldnt find PauseMenuBackground");
        if (menuSections != null)
            sections.Add(menuSections);
        if (audioSection != null)
            sections.Add(audioSection);
        if (saveLoadSection != null)
            sections.Add(saveLoadSection);
        if (creditSection != null)
            sections.Add(creditSection);
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

        backButton = FindButton("BackButton", () =>
        {
            EventManager.PlaySound?.Invoke("switch1", true);
            deleteButtonPressed = false;
            if (currentSection == menuSections || (sceneChanger.MainMenuDone == false && currentSection != confirmSection))
            {
                EventManager.DisplayMenuButton?.Invoke(true);
                EventManager.ReactivateMainMenuButtons?.Invoke();
                Time.timeScale = 1;
                gameObject.SetActive(false);
            }
            else if (currentSection == confirmSection)
            {
                ChangeMenuSection(saveLoadSection);                
            }
            else
                ChangeMenuSection(menuSections);
        });
        backButtonText = backButton.GetComponentInChildren<TextMeshProUGUI>();

        saveButton = FindButton("SaveButton", () =>
        {
            lastSaveLoadOption = "save";
            ChangeMenuSection(saveLoadSection);
            UpdateSaveLoadButtons("save");
            EventManager.PlaySound?.Invoke("switch1", true);
        });
        // Hide the save button initally in the main menu
        saveButton?.gameObject.SetActive(false);

        FindButton("LoadButton", () =>
        {
            lastSaveLoadOption = "load";
            ChangeMenuSection(saveLoadSection);
            UpdateSaveLoadButtons("load");
            EventManager.PlaySound?.Invoke("switch1", true);
        });

        deleteButton = FindButton("DeleteButton", () =>
        {
            deleteButtonPressed = true;
            UpdateSaveLoadButtons("delete");
            ChangeMenuSection(saveLoadSection);
            EventManager.PlaySound?.Invoke("switch1", true);
        });
        deleteButton?.gameObject.SetActive(false);

        FindButton("OptionsButton", () =>
        {
            ChangeMenuSection(audioSection);
            EventManager.PlaySound?.Invoke("switch1", true);
        });

        mainMenuButton = FindButton("MainMenuButton", () =>
        {
            EventManager.PlaySound?.Invoke("switch1", true);
            ChangeMenuSection(confirmSection);
            ChangeObjectText(confirmText, "Return to the Main Menu?");
            ChangeObjectText(pauseMenuText, "Settings");

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
            EventManager.PlaySound?.Invoke("switch1", true);
            ChangeMenuSection(confirmSection);
            ChangeObjectText(confirmText, "Close the Game?");
            ChangeObjectText(pauseMenuText, "Settings");

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
            EventManager.PlaySound?.Invoke("switch1", true);
            confirmAction?.Invoke();
        });

        FindButton("NoButton", () =>
        {
            EventManager.PlaySound?.Invoke("switch1", true);
            cancelAction?.Invoke();
        });

        FindButton("EasyButton", () =>
        {
            EventManager.PlaySound?.Invoke("switch1", true);
            gameManager.gameData.gameMode = GameData.GameMode.Easy;
            confirmAction?.Invoke();
        });
        FindButton("NormalButton", () =>
        {
            EventManager.PlaySound?.Invoke("switch1", true);
            gameManager.gameData.gameMode = GameData.GameMode.Normal;
            confirmAction?.Invoke();
        });
        FindButton("HardButton", () =>
        {
            EventManager.PlaySound?.Invoke("switch1", true);
            gameManager.gameData.gameMode = GameData.GameMode.Hard;
            confirmAction?.Invoke();
        });

        // Save and Load Slot Buttons
        slot1Button = FindObject<Button>("Slot1Button");
        slot2Button = FindObject<Button>("Slot2Button");
        slot3Button = FindObject<Button>("Slot3Button");
        UpdateSlotInformation();

    }

    private void FindManagers()
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

        GameObject sceneChangerObject = GameObject.Find("SceneChanger");
        if (sceneChangerObject == null)
        {
            Debug.LogError("SceneChanger GameObject not found!");
            return;
        }

        sceneChanger = sceneChangerObject.GetComponent<SceneChanger>();
        if (sceneChanger == null)
        {
            Debug.LogError("SceneChanger component not found!");
        }

        Canvas prefabCanvas = GetComponentInChildren<Canvas>();
        if (prefabCanvas != null)
        {
            prefabCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            prefabCanvas.worldCamera = Camera.main;
            prefabCanvas.sortingLayerID = SortingLayer.NameToID("Media");
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
                if (isLoadingSettings) return;

                EventManager.PlaySound?.Invoke("switch1", true); 
                EventManager.ToggleFullScreenState();
                fullScreenActiveText.text = isOn ? "On" : "Off";
                Screen.fullScreen = fullScreenToggle.isOn;
                SavePersistentSettings();
            });
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
                if (isLoadingSettings) return;
                
                EventManager.PlaySound?.Invoke("switch1", true); 
                EventManager.ToggleGrayscaleState();
                grayscaleActiveText.text = isOn ? "On" : "Off";
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

        // Sound Sliders
        masterVolumeSlider = FindComponentByName<Slider>("MasterVolume");
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(value =>
            {
                audioManager.UpdateMasterVolume(value);
                SavePersistentSettings();
            });
        }

        musicVolumeSlider = FindComponentByName<Slider>("MusicVolume");
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(value =>
            {
                audioManager.UpdateMusicVolume(value);
                SavePersistentSettings();
            });
        }

        sfxVolumeSlider = FindComponentByName<Slider>("SFXVolume");
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(value =>
            {
                audioManager.UpdateSFXVolume(value);
                SavePersistentSettings();
            });
        }

        // Text Speed Slider
        textSpeedSlider = FindComponentByName<Slider>("TextSpeed");
        if (textSpeedSlider != null)
        {
            textSpeedSlider.onValueChanged.AddListener(value =>
            {
                EventManager.SetTextSpeed?.Invoke(value);
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
                if (isLoadingSettings) return;

                audioManager.MuteToggle(isOn);
                muteActiveText.text = isOn ? "On" : "Off";
                EventManager.PlaySound?.Invoke("switch1", true);
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
                if (isLoadingSettings) return;

                bool subtitleIsOn = PlayerPrefs.GetInt("SubtitleState", 0) == 1;
                PlayerPrefs.SetInt("SubtitleState", !subtitleIsOn ? 1 : 0);
                subtitleActiveText.text = subtitleIsOn ? "On" : "Off";

                EventManager.SubtitleToggle?.Invoke();
                EventManager.PlaySound?.Invoke("switch1", true);
                SavePersistentSettings();
            });
        }
    }

    public void SavePersistentSettings()
    {
        if (isLoadingSettings) return; // Prevent saving while loading settings

        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        PlayerPrefs.SetFloat("TextSpeed", textSpeedSlider.value);
        PlayerPrefs.SetInt("MuteState", muteToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("SubtitleState", subtitleToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("GrayState", grayscaleToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("FullScreenState", fullScreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void LoadPersistentSettings()
    {
        isLoadingSettings = true;  // Prevent SavePersistentSettings from being called

        float master = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        float text = PlayerPrefs.GetFloat("TextSpeed", 1.0f);

        masterVolumeSlider.value = master;
        musicVolumeSlider.value = music;
        sfxVolumeSlider.value = sfx;
        textSpeedSlider.value = text;

        audioManager.UpdateMasterVolume(master);
        audioManager.UpdateMusicVolume(music);
        audioManager.UpdateSFXVolume(sfx);
        EventManager.SetTextSpeed?.Invoke(text);

        int muteOn = PlayerPrefs.GetInt("MuteState", 0);
        muteToggle.isOn = muteOn == 1;
        audioManager.MuteToggle(muteToggle.isOn);

        bool subtitleOn = PlayerPrefs.GetInt("SubtitleState", 0) == 1;
        subtitleToggle.isOn = subtitleOn;

        int grayOn = PlayerPrefs.GetInt("GrayState", 0);
        grayscaleToggle.isOn = grayOn == 1;

        int fullScreenOn = PlayerPrefs.GetInt("FullScreenState", 0);
        fullScreenToggle.isOn = fullScreenOn == 1;
        Screen.fullScreen = fullScreenToggle.isOn;
        
        isLoadingSettings = false;
    }


    // Options Menu Functions --------------------------------------//
    // -------------------------------------------------------------//
    private void ChangeMenuSection(GameObject section)
    {
        UpdateSlotInformation();
        currentSection = section;

        // Change backButton text to coincide with action, done being closing menu
        backButtonText.text = section == menuSections ? "Done" : "Back";
        if (sceneChanger.MainMenuDone == false)
            backButtonText.text = "Done";
        if (section == confirmSection)
            backButtonText.text = "Back";
        backButton.gameObject.SetActive(section != confirmSection);
        
        difficultyButtons.SetActive(false);
        confirmButtons.SetActive(true);

        // Show Save File Deletion Button
        deleteButton.gameObject.SetActive(section == saveLoadSection);
        ChangeObjectText(pauseMenuText, "Settings");

        if (section == audioSection)
            ChangeObjectText(pauseMenuText, "Accessibility");
        else if (section == saveLoadSection || section == confirmSection)
        {
            ChangeObjectText(pauseMenuText, "Choose a save file");
            if (deleteButtonPressed)
            {
                ChangeObjectText(pauseMenuText, "Select a save to delete");

            }
        }
        if (section == creditSection)
        {
            ChangeObjectText(pauseMenuText, "Credits");
            backButtonText.text = "Done";
        }

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
        lastSaveLoadOption = "load";
        switch (option.ToLower())
        {
            case "load":
                UpdateSaveLoadButtons("load");
                ChangeMenuSection(saveLoadSection);
                break;
            case "options":
                ChangeMenuSection(audioSection);
                break;
            case "credits":
                ChangeMenuSection(creditSection);
                break;
            default:
                //saveButton?.gameObject.SetActive(true);
                mainMenuButton?.gameObject.SetActive(true);
                ChangeMenuSection(menuSections);
                break;
        }
    }

    private void ChangeObjectText(TextMeshProUGUI gameObject, string text)
    {
        gameObject.text = text;
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
                EventManager.PlaySound?.Invoke("switch1", true);
                if (saveButtonPressed == "save")
                    HandleSaveSlot(slotIndex);
                else if (saveButtonPressed == "load")
                    HandleLoadSlot(slotIndex);
                else
                    HandleDeleteSlot(slotIndex);
            });
        }
    }

    private void ConfirmMenuChooseDifficulty(int slot)
    {
        ChangeMenuSection(confirmSection);
        ChangeObjectText(confirmText, $"Select Game Difficulty");
        confirmButtons.SetActive(false);
        difficultyButtons.SetActive(true);
        backButton.gameObject.SetActive(true);

        confirmAction = () =>
        {
            gameManager.gameData.saveSlot = slot;
            gameManager.gameData.day = 1;
            SaveSystem.SaveGame(slot, gameManager.gameData);
            EventManager.BeginNewGame?.Invoke();
            gameObject.SetActive(false);
            deleteButtonPressed = false;
            Time.timeScale = 1;
            //ChangeMenuSection(saveLoadSection);
        };   
    }

    private void HandleSaveSlot(int slot)
    {
        if (SaveSystem.SaveExists(slot))
        {
            ChangeMenuSection(confirmSection);
            ChangeObjectText(confirmText, $"Create A New Save Over File {slot}?");
            confirmButtons.SetActive(true);
            difficultyButtons.SetActive(false);
            deleteButton.gameObject.SetActive(false);
            backButton.gameObject.SetActive(true);

            confirmAction = () =>
            {
                ConfirmMenuChooseDifficulty(slot);
            };
            cancelAction = () =>
            {
                ChangeMenuSection(saveLoadSection);
            };
        }
        else
        {
            ConfirmMenuChooseDifficulty(slot);
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
            ChangeMenuSection(confirmSection);
            ChangeObjectText(confirmText, $"Load Game File {slot}?");
            confirmButtons.SetActive(true);
            difficultyButtons.SetActive(false);
            backButton.gameObject.SetActive(true);

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
            ChangeMenuSection(confirmSection);
            ChangeObjectText(pauseMenuText, "Select a save to delete");
            ChangeObjectText(confirmText, $"Delete Game File {slot}?");
            confirmButtons.SetActive(true);
            difficultyButtons.SetActive(false);

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
        if (slot1Button == null || slot2Button == null || slot3Button == null)
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

            if (SaveSystem.SaveExists(slotIndex))
            {
                GameData gameSlot = SaveSystem.LoadGame(slotIndex);
                if (gameSlot != null)
                {
                    // Convert playtime to "H:MM:SS" format
                    string time = SecondMinuteHourConversion(gameSlot.playTime);
                    buttonText.text = $"Save Slot {slotIndex}\n\n{time}\n\nDay: {gameSlot.day}\n\nDifficulty: {gameSlot.gameMode}";
                }
                else
                {
                    buttonText.text = "Save Data is Corrupted";
                    Debug.Log($"Failed to get Game Slot {slotIndex}");
                }
            }
            else
                buttonText.text = $"Save Slot {slotIndex}\n\nOpen Position";
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

    public void NewStartGame()
    {
        lastSaveLoadOption = "save";
        UpdateSaveLoadButtons("save");
        ChangeMenuSection(saveLoadSection);
    }

    void OnEnable()
    {
        EventManager.OptionsChanger += OptionsChanger;
        EventManager.NewStartGame += NewStartGame;
    }

    void OnDisable()
    {
        EventManager.OptionsChanger -= OptionsChanger;
        EventManager.NewStartGame -= NewStartGame;
    }
}
