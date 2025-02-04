using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Collections;
using Unity.VisualScripting;

public class SceneManager : MonoBehaviour
{
    [SerializeField] private GameObject UITextBox;
    [SerializeField] private Sprite FemaleNewsAnchor;
    [SerializeField] private Sprite MaleNewsAnchor;
    private GameObject currentTextBox;
    private TextMeshProUGUI TextBox;
    private Button NextButton;
    private Image backgroundImage;
    private Image FadeOutImage;
    
    private int dayCounter = 0;
    private int linePos = 0;
    private string[] currentLines;

    void Start()
    {
        // Comment out to skip right to the job
        // Possible addition to add a skip to job button after one playthrough?
        LoadDayStart();
    }

    private void LoadDayStart() {
        
        dayCounter++;
        currentTextBox = Instantiate(UITextBox);

        if (currentTextBox == null)
        {
            Debug.LogError("currentTextBox is null.");
            return;
        }

        ShowDialogTextBox();
        LoadJsonFromFile();
        ShowBackgroundImage();
        StartCoroutine(FadeInDayStart());
    }

    private void ShowBackgroundImage()
    {
        backgroundImage = currentTextBox.transform.Find("BackgroundImage").GetComponent<Image>();
        if(backgroundImage == null)
        {
            Debug.Log("Failed to find Image component");
            return;
        }
        backgroundImage.sprite = FemaleNewsAnchor; 

        FadeOutImage = currentTextBox.transform.Find("FadeOutImage").GetComponent<Image>();
        if(FadeOutImage == null)
        {
            Debug.Log("Failed to find FadeOut component");
            return;
        }
    }

    private void ShowDialogTextBox()
    {
        TextBox = currentTextBox.transform.Find("TextBox")?.GetComponent<TextMeshProUGUI>();
        if (TextBox == null)
        {
            Debug.LogError("Failed to find TextMeshProUGUI component.");
            return;
        }

        NextButton = currentTextBox.transform.Find("NextTextButton")?.GetComponent<Button>();
        if (NextButton == null)
        {
            Debug.LogError("Failed to find Button component.");
            return;
        }
        NextButton.onClick.AddListener(ReadNextLine);
    }

    private void LoadJsonFromFile()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "GameText.json");
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

        if (jsonObject != null && jsonObject.newsCasterIntro.Count > 0)
        {
            currentLines = GetLinesForDay(jsonObject.newsCasterIntro, dayCounter);
            linePos = 0;
            ReadNextLine();
        }
        else
        {
            Debug.LogError("JSON parsing failed or empty list.");
        }
    }

    private string[] GetLinesForDay(List<Entry> entries, int day)
    {
        foreach (var entry in entries)
        {
            if (entry.day == day)
            {
                return entry.lines;
            }
        }
        return new string[0];
    }

    private void ReadNextLine()
    {
        if (linePos < currentLines.Length)
        {
            TextBox.text = currentLines[linePos];
            linePos++;
        }
        else
        {
            // Destroy UI object or hide.
            Debug.Log("End of dialogue.");
            StartCoroutine(FadeOutAndDestroy());
        }
    }

    private float fadeDuration = 3f; // Adjustable fade duration
    private float waitTime = 1f; // Time to wait before fading back in

    private IEnumerator FadeInDayStart()
    {
        // Start with FadeOutImage dark then fade in
        yield return new WaitForSeconds(waitTime);
        yield return StartCoroutine(FadeImage(FadeOutImage, 1f, 0f, fadeDuration));
        FadeOutImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeOutAndDestroy()
    {
        // Set true so that it can be visible and fade in and out
        FadeOutImage.gameObject.SetActive(true);

        // Fade to black
        yield return StartCoroutine(FadeImage(FadeOutImage, 0f, 1f, fadeDuration));

        // Hide UI elements after fade-out
        backgroundImage.gameObject.SetActive(false);
        TextBox.gameObject.SetActive(false);
        NextButton.gameObject.SetActive(false);
        Image TextBoxBackground = currentTextBox.transform.Find("TextBoxBackground").GetComponent<Image>();
        TextBoxBackground.gameObject.SetActive(false);

        // Wait before fading back in
        yield return new WaitForSeconds(waitTime);

        // Fade to next scene
        yield return StartCoroutine(FadeImage(FadeOutImage, 1f, 0f, fadeDuration));

        Destroy(currentTextBox);
        currentTextBox = null;
        BeginWorkDay();
    }

    private IEnumerator FadeImage(Image image, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        Color color = image.color;
        color.a = startAlpha;
        image.color = color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            image.color = color;
            yield return null;
        }

        color.a = endAlpha;
        image.color = color;
    }


    private void BeginWorkDay()
    {
        throw new NotImplementedException();
    }

    [System.Serializable]
    private class Wrapper
    {
        public List<Entry> newsCasterIntro;
    }

    [System.Serializable]
    private class Entry
    {
        public int day;
        public string[] lines;
    }
}
