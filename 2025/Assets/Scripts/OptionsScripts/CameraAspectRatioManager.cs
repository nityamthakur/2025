// Code Provided by Shinimini 
// https://gamedev.stackexchange.com/questions/86707/how-to-lock-aspect-ratio-when-resizing-game-window-in-unity

using UnityEngine;

public class CameraAspectRatioManager : MonoBehaviour
{
    [SerializeField] private int widthAspect = 16;
    [SerializeField] private int heightAspect = 9;
    [Tooltip("After every how many frames to rescale the windows size.")]
    [SerializeField] private int updateAspectDelay = 100;

    private int frame = 0;

    private int lastWidth = 0;
    private int lastHeight = 0;

    private void Update()
    {
        frame++;

        if (frame % updateAspectDelay != 0) { return; }

        var width = Screen.width;
        var height = Screen.height;

        if (lastWidth != width) // if the user is changing the width
        {
            // update the height
            float heightAccordingToWidth = (float) width / widthAspect * heightAspect;
            Screen.SetResolution(width, (int)Mathf.Round(heightAccordingToWidth), false);
        }
        else if (lastHeight != height) // if the user is changing the height
        {
            // update the width
            float widthAccordingToHeight = (float) height / heightAspect * widthAspect;
            Screen.SetResolution((int)Mathf.Round(widthAccordingToHeight), height, false);
        }

        lastWidth = width;
        lastHeight = height;
    }
}