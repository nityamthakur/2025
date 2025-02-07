using System;
using System.Collections;
using UnityEngine;

public static class EventManager
{
    public static Action<GameObject> OnMediaDestroyed; // Event for media object destruction
    public static Action NextScene; // Event for beginning the next scene
    public static Action FadeIn; // Trigger Fade in animation as a coroutine
    public static Action FadeOut; // Trigger Fade out animation as a coroutine
}
