using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class SubtitleManager : MonoBehaviour
{
    [SerializeField] private SceneChanger sceneChanger;
    private GameObject fadingScreen;
    private TextMeshProUGUI subtitleTextObject;
    private RectTransform subtitleBackground;
    private Coroutine subtitleTime;
    private float paddingX = 20f; // Horizontal padding
    private float paddingY = 10f; // Vertical padding
    private Dictionary<string, string> subtitleDictionary = new Dictionary<string, string>();
    private bool subtitlesOn = false;

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
            {
                subtitleTextObject = textTransform.GetComponent<TextMeshProUGUI>();
            }
            else
                Debug.LogError("Subtitle Text (SubtitleText) not found inside fadingScreen!");
        }

        SubtitleToggle(true);
        LoadJsonFromFile();
    }

    public void ShowSubtitle(string soundName, float soundTime)
    {
        if (subtitleDictionary.TryGetValue(soundName, out string subtitleText) && subtitlesOn)
        {
            //Debug.Log("Showing subtitle.");
            ShowHideSubtitle(true);

            subtitleTextObject.text = $"[ {subtitleText} ]";

            // Force a layout rebuild so the ContentSizeFitter updates
            LayoutRebuilder.ForceRebuildLayoutImmediate(subtitleTextObject.rectTransform);

            // Resize background to match text
            Vector2 textSize = subtitleTextObject.rectTransform.sizeDelta;
            subtitleBackground.sizeDelta = new Vector2(textSize.x + paddingX, textSize.y + paddingY);

            if (subtitleTime != null)
            {
                StopCoroutine(subtitleTime);
            }
            subtitleTime = StartCoroutine(HideSubtitle(soundTime));
        }
    }

    public void ShowCustomSubtitle(string subtitle)
    {
        //Debug.Log("Showing subtitle.");
        ShowHideSubtitle(true);

        subtitleTextObject.text = $"[ {subtitle} ]";

        // Force a layout rebuild so the ContentSizeFitter updates
        LayoutRebuilder.ForceRebuildLayoutImmediate(subtitleTextObject.rectTransform);

        // Resize background to match text
        Vector2 textSize = subtitleTextObject.rectTransform.sizeDelta;
        subtitleBackground.sizeDelta = new Vector2(textSize.x + paddingX, textSize.y + paddingY);

        if (subtitleTime != null)
        {
            StopCoroutine(subtitleTime);
        }
        subtitleTime = StartCoroutine(HideSubtitle(5f));
    }

    public IEnumerator HideSubtitle(float soundTime)
    {
        // Show the subtitle for as long as the sound is active. Min 2 sec, Max 5 sec
        float minMaxTime = Mathf.Clamp(soundTime, 2f, 5f);
        yield return new WaitForSecondsRealtime(minMaxTime); // Uses real time, ignoring options pause
        ShowHideSubtitle(false);
    }

    private void LoadJsonFromFile()
    {
        // Check if Json is found in StreamingAssets folder
        string path = Path.Combine(Application.streamingAssetsPath, "Subtitles.json");
        if (!File.Exists(path))
        {
            Debug.LogError("JSON file not found at: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        ParseJson(json);
    }

    private void ParseJson(string json)
    {
        var jsonObject = JsonUtility.FromJson<Wrapper>(json);

        if (jsonObject != null && jsonObject.subtitles.Count > 0)
        {
            subtitleDictionary = GetSubtitles(jsonObject.subtitles);
        }
        else
        {
            Debug.LogError("JSON parsing failed or empty list.");
        }
    }

    private Dictionary<string, string> GetSubtitles(List<Entry> entries)
    {
        Dictionary<string, string> subtitles = new Dictionary<string, string>();

        foreach (var entry in entries)
        {
            if (!subtitles.ContainsKey(entry.sound.ToLower()))
            {
                subtitles[entry.sound.ToLower()] = entry.text; // Store subtitles using "sound" as the key
            }
            else
            {
                Debug.LogWarning($"Duplicate subtitle entry found for sound '{entry.sound}', ignoring duplicate.");
            }
        }

        return subtitles;
    }

    public void ShowHideSubtitle( bool isOn)
    {
        subtitleBackground.gameObject.SetActive(isOn);
        subtitleTextObject.gameObject.SetActive(isOn);
    }

    public void SubtitleToggle(bool isOn)
    {
        subtitlesOn = isOn;
        ShowHideSubtitle(isOn);
    }

    private void OnEnable()
    {
        EventManager.ShowCustomSubtitle += ShowCustomSubtitle;
    }

    private void OnDisable()
    {
        EventManager.ShowCustomSubtitle -= ShowCustomSubtitle;
    }

    [System.Serializable]
    private class Wrapper
    {
        public List<Entry> subtitles;
    }

    [System.Serializable]
    private class Entry
    {
        public string sound;
        public string text;
    }
}