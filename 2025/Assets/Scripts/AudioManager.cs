using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AudioManager : MonoBehaviour
{
    private AudioSource musicSource;
    [SerializeField] private SubtitleManager subtitleManager;
    [SerializeField] private AudioSource sfxPrefab;
    private Dictionary<string, AudioClip> musicDict = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxDict = new Dictionary<string, AudioClip>();
    public float masterVolume = 1.0f;
    public float musicVolume = 1.0f;
    public float sfxVolume = 1.0f;
    public float muteVolume = 1.0f;
    private Coroutine currentMusicCoroutine; // Store the currently running coroutine
    public bool canPlaySounds = false; // Prevent sounds from being played until after game has started

    public void Initialize()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;  
        musicSource.volume = musicVolume; 
        musicSource.playOnAwake = false;

        AudioClip[] music = Resources.LoadAll<AudioClip>("Audio/Music");
        foreach (var clip in music)
        {
            musicDict[clip.name.ToLower()] = clip;
        }
        //Debug.Log($"Loaded {musicDict.Count} music tracks from Resources/Audio/Music/");

        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio/SFX");
        foreach (var clip in clips)
        {
            sfxDict[clip.name.ToLower()] = clip;
        }
        //Debug.Log($"Loaded {sfxDict.Count} sound effects from Resources/Audio/SFX/");
    }

    private void OnEnable()
    {
        EventManager.PlayMusic += PlayMusic;
        EventManager.PauseResumeMusic += PauseResumeMusic;
        EventManager.StopMusic += StopMusic;
        EventManager.PlaySound += PlaySound;
        EventManager.IsMusicPlaying += IsMusicPlaying;
    }

    private void OnDisable()
    {
        EventManager.PlayMusic -= PlayMusic;
        EventManager.PauseResumeMusic -= PauseResumeMusic;
        EventManager.StopMusic -= StopMusic;
        EventManager.PlaySound -= PlaySound;
        EventManager.IsMusicPlaying += IsMusicPlaying;
    }

    public bool IsMusicPlaying()
    {
        return currentMusicCoroutine != null;
    }

    public void PlayMusic(string soundName)
    {
        if(!canPlaySounds)
        {
            return;
        }

        bool soundExists = musicDict.TryGetValue(soundName.ToLower(), out AudioClip sound);
        if (!soundExists || sound == null)
        {
            Debug.Log($"Music not found: {soundName}");
            return;
        }

        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }

        musicSource.clip = sound;
        musicSource.Play();

        if (currentMusicCoroutine != null)
        {
            StopCoroutine(currentMusicCoroutine);
        }

        currentMusicCoroutine = StartCoroutine(FadeInMusic());

        // Call subtitle
        subtitleManager.ShowSubtitle(soundName.ToLower(), sound.length);    
    }

    public void PauseResumeMusic()
    {
        if (musicSource.isPlaying)
            musicSource.Pause();
        else
            musicSource.UnPause();
    }

    public void StopMusic()
    {
        if (currentMusicCoroutine != null)
        {
            StopCoroutine(currentMusicCoroutine);
        }

        currentMusicCoroutine = StartCoroutine(FadeOutMusic());
    }

    public void PlaySound(string soundName)
    {
        //Debug.Log($"playing {soundName}. canPlaySounds: {canPlaySounds}");
        if(!canPlaySounds)
        {
            return;
        }

        bool soundExists = sfxDict.TryGetValue(soundName.ToLower(), out AudioClip sound);
        if (!soundExists || sound == null)
        {
            Debug.Log($"Sound not found: {soundName}");
            return;
        }

        if (sfxPrefab == null)
        {
            Debug.Log("sfxPrefab is not assigned in the Inspector!");
            return;
        }

        // Create a new AudioSource to allow multiple sounds to play
        AudioSource audioSource = Instantiate(sfxPrefab, transform);
        audioSource.clip = sound;
        audioSource.volume = masterVolume * sfxVolume * muteVolume;
        audioSource.Play();

        // Destroy after sound finishes playing
        Destroy(audioSource.gameObject, sound.length);

        // Call subtitle
        subtitleManager.ShowSubtitle(soundName.ToLower(), sound.length);
    }

    private IEnumerator FadeInMusic()
    {
        float duration = 2.0f;
        musicSource.volume = 0f;

        while (musicSource.volume < masterVolume * musicVolume * muteVolume)
        {
            musicSource.volume += Time.deltaTime / duration;
            yield return null;
        }

        musicSource.volume = masterVolume * musicVolume * muteVolume;
    }

    private IEnumerator FadeOutMusic()
    {
        float duration = 2.0f;

        while (musicSource.volume > 0)
        {
            musicSource.volume -= Time.deltaTime / duration;
            yield return null;
        }

        musicSource.volume = 0;
        musicSource.Stop();
    }

    public void UpdateMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp(volume, 0f, 1f); 
        musicSource.volume = masterVolume * musicVolume * muteVolume;
    }

   public void UpdateMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp(volume, 0f, 1f); 
        musicSource.volume = masterVolume * musicVolume * muteVolume;
    }

   public void UpdateSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp(volume, 0f, 1f); 
    }

    public void MuteToggle(bool isOn)
    {
        if(isOn)
            muteVolume = 0f;
        else
            muteVolume = 1.0f;

        musicSource.volume = masterVolume * musicVolume * muteVolume;
    }
}