using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    private AudioManager audioManager;

    public void AddAudioComponent(AudioManager audioObject)
    {
        audioManager = audioObject;
    }

    private void Start()
    {
        // Close button setup
        Button closeButton = FindComponentByName<Button>("CloseMenuButton");
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() =>
            {
                EventManager.ReactivateMainMenuButtons?.Invoke(); 
                EventManager.PlaySound?.Invoke("switch1"); 
                this.gameObject.SetActive(false);
            });
        }

        GrayscaleSetUp();
        AudioSetUp();
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
}
