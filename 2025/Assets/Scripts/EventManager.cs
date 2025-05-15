using System;
using UnityEngine;

public static class EventManager
{
    public static Action<GameObject> OnMediaDestroyed; // Event for media object destruction
    public static Action OnImageDestroyed; // Event for non media object destruction
    public static Action<bool> ShowHideRentNotices;

    public static Action NextScene; // Event for beginning the next scene
    public static Action FadeIn; // Trigger Fade in animation as a coroutine
    public static Action FadeOut; // Trigger Fade out animation as a coroutine

    // Used for ensuring the media gets enters and leaves behind certain screen elements
    public static Action ShowDeskOverlay;
    public static Action HideDeskOverlay;
    public static Action ShowLightsOutImage;
    public static Action HideLightsOutImage;
    public static Action<string, bool> GlowingBoxShow;

    public static Action<bool> DisplayMenuButton;
    public static Func<bool> IsMusicPlaying;

    // MainMenuScene Events
    public static Action BeginNewGame;

    // OptionsMenu Events
    public static Action<string> OptionsChanger;
    public static Action NewStartGame;
    public static Action OpenOptionsMenu;
    public static Action ReactivateMainMenuButtons;

    // AudioManage Events
    public static Action<string> ShowCustomSubtitle;
    public static Action<string> PlayMusic;
    public static Action PauseResumeMusic;
    public static Action StopMusic;
    public static Action<string, bool> PlaySound;

    public static Action<bool> ToggleGrayscale;
    public static bool IsGrayscale { get; private set; } = false;
    public static void ToggleGrayscaleState()
    {
        IsGrayscale = !IsGrayscale;
        ToggleGrayscale?.Invoke(IsGrayscale);
    }

    public static Action<bool> ToggleFullScreen;
    public static bool IsFullScreen { get; private set; } = false;
    public static void ToggleFullScreenState()
    {
        IsFullScreen = !IsFullScreen;
        ToggleFullScreen?.Invoke(IsFullScreen);
    }

    public static Action<float> SetTextSpeed;
    public static Action<bool> ShowCorrectBuzzer;
    public static Action<int> GoToShop;

}
