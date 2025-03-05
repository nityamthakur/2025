using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

public class SubtitleManager : MonoBehaviour
{
    [SerializeField] private SceneChanger sceneChanger;
    private GameObject fadingScreen;
    private TextMeshProUGUI subtitleText;
    private RectTransform subtitleBackground;
    private Coroutine subtitleTime;
    private float paddingX = 20f; // Horizontal padding
    private float paddingY = 10f; // Vertical padding

    public void Initialize()
    {
        fadingScreen = sceneChanger.FadingScreen;

        if (fadingScreen != null)
        {
            Transform bgTransform = fadingScreen.transform.Find("SubtitleBackground");
            if (bgTransform != null)
                subtitleBackground = bgTransform.GetComponent<RectTransform>();
            else
                Debug.LogError("Subtitle Background (SubtitleBackground) not found inside fadingScreen!");


            Transform textTransform = bgTransform.transform.Find("SubtitleText");
            if (textTransform != null)
                subtitleText = textTransform.GetComponent<TextMeshProUGUI>();
            else
                Debug.LogError("Subtitle Text (SubtitleText) not found inside fadingScreen!");
        }
    }

    public void ShowSubtitle(string text)
    {
        if(subtitleText)
        subtitleText.text = text;

        // Force a layout rebuild so the ContentSizeFitter updates
        LayoutRebuilder.ForceRebuildLayoutImmediate(subtitleText.rectTransform);

        // Resize background to match text
        Vector2 textSize = subtitleText.rectTransform.sizeDelta;
        subtitleBackground.sizeDelta = new Vector2(textSize.x + paddingX, textSize.y + paddingY);

        gameObject.SetActive(true);
        subtitleTime = null;
        //subtitleTime = StartCoroutine(HideSubtitle);
    }

    public IEnumerator HideSubtitle()
    {
        // 
        gameObject.SetActive(false);
        yield return null;

    }
}
