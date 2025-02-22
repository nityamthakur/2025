using System;
using UnityEngine;

public static class EventManager
{
    public static Action<GameObject> OnMediaDestroyed; // Event for media object destruction
    public static Action NextScene; // Event for beginning the next scene
    public static Action FadeIn; // Trigger Fade in animation as a coroutine
    public static Action FadeOut; // Trigger Fade out animation as a coroutine
    
    // Used for ensuring the media gets enters and leaves behind certain screen elements
    public static Action ShowDeskOverlay;
    public static Action HideDeskOverlay;

    public static Action<string> PlayMusic;
    public static Action StopMusic;
    public static Action<string> PlaySound;
    public static Action<int, int, int> UpdateVolume;

    public static Action<bool> ToggleGrayscale;
    public static bool IsGrayscale { get; private set; } = false;
    public static void ToggleGrayscaleState()
    {
        IsGrayscale = !IsGrayscale;
        ToggleGrayscale?.Invoke(IsGrayscale);
    }

}
