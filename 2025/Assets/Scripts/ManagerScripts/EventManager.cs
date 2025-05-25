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

    // FadingScreen Events
    public static Action<bool> DisplayDeskOverlay;
    public static Action<bool> DisplayLightsOutImage;
    public static Action<bool> DisplayMenuButton;


    //
    public static Action<string, bool> GlowingBoxShow;
    public static Func<bool> IsMusicPlaying;

    // MainMenuScene Events
    public static Action BeginNewGame;

    // OptionsMenu Events
    public static Action<string> OptionsChanger;
    public static Action NewStartGame;
    public static Action OpenOptionsMenu;
    public static Action ReactivateMainMenuButtons;

    // AudioManage Events
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

    // Subtitle Events
    public static Action<string> ShowCustomSubtitle;
    public static Action<string, float> ShowSubtitle;

    // ------------------
    public static Action<bool> ShowCorrectBuzzer;
    public static Action GoToShop;

    // Clock Event
    public static Action<float> StartClockMovement;
    public static Action StopClockMovement;

    // Camera Events
    public static Action<Transform, float, float> ZoomCamera;
    public static Action<float> ResetCamera;
    public static Action<bool> CameraZoomed;
}
