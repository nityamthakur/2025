using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Collections;

public class DayStartScene : MonoBehaviour
{
    [SerializeField] private GameObject UITextBox;
    [SerializeField] private Sprite[] FemaleNewsAnchor;
    [SerializeField] private Sprite[] MaleNewsAnchor;
    [SerializeField] private Sprite[] MaleNewsAnchor2;
    [SerializeField] private Sprite[] jobLetter;
    [SerializeField] private float frameInterval = 0.5f;

    private GameObject currentTextBox;
    private TextMeshProUGUI TextBox;
    private Button nextButton;
    private Image backgroundImage;
    //private int currDay = 0;
    private int linePos = 0;
    private Line[] currentLines;
    private GameManager gameManager;
    private Coroutine animationCoroutine;

    public void Initialize()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void LoadDayStart()
    {
        currentTextBox = Instantiate(UITextBox);

        if (currentTextBox == null)
        {
            Debug.LogError("currentTextBox is null.");
            return;
        }

        SetUpDayStart();

        if (!EventManager.IsMusicPlaying())
            EventManager.PlayMusic?.Invoke("menu");

        EventManager.FadeIn?.Invoke();
        EventManager.DisplayMenuButton?.Invoke(true);
    }

    private void SetUpDayStart()
    {
        backgroundImage = currentTextBox.transform.Find("BackgroundImage").GetComponent<Image>();
        if (backgroundImage == null)
        {
            Debug.Log("Failed to find Image component");
            return;
        }

        TextBox = currentTextBox.transform.Find("TextBox").GetComponent<TextMeshProUGUI>();
        if (TextBox == null)
        {
            Debug.LogError("Failed to find TextMeshProUGUI component.");
            return;
        }

        TextMeshProUGUI dayText = currentTextBox.transform.Find("DayText").GetComponent<TextMeshProUGUI>();
        if (dayText == null)
        {
            Debug.LogError("Failed to find dayText component.");
            return;
        }
        else
            dayText.text = $"Day {gameManager.gameData.day}";

        nextButton = currentTextBox.transform.Find("NextTextButton").GetComponent<Button>();
        if (nextButton == null)
        {
            Debug.LogError("Failed to find Button component.");
            return;
        }
        nextButton.onClick.AddListener(() =>
        {
            EventManager.PlaySound?.Invoke("switch1");
            ReadNextLine();
        });

        LoadJsonFromFile();
    }

    private void LoadJsonFromFile()
    {
        // Check if Json is found in StreamingAssets folder
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
            currentLines = GetLinesForDay(jsonObject.newsCasterIntro, gameManager.gameData.GetCurrentDay());

            linePos = 0;
            ReadNextLine();
        }
        else
        {
            Debug.LogError("JSON parsing failed or empty list.");
        }
    }

    private Line[] GetLinesForDay(List<Entry> entries, int day)
    {
        foreach (var entry in entries)
        {
            if (entry.day == day)
            {
                return entry.lines;
            }
        }
        return new Line[0];
    }

    private void ReadNextLine()
    {
        if (linePos < currentLines.Length)
        {
            // Get the current line object
            Line currentLine = currentLines[linePos];

            // Set the text in the dialogue box
            TextBox.text = currentLine.text;
            ChangeSpeaker(currentLine);
            linePos++;
        }
        else
        {
            nextButton.interactable = false;
            EventManager.DisplayMenuButton?.Invoke(false);
            StartCoroutine(NextScene());
        }
    }

    private void ChangeSpeaker(Line currentLine)
    {
        // Change the speaker image based on who is speaking
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        switch (currentLine.speaker.ToLower())
        {
            case "femalenewsanchor":
                animationCoroutine = StartCoroutine(CycleBackgroundFrames(FemaleNewsAnchor));
                break;
            case "malenewsanchor":
                animationCoroutine = StartCoroutine(CycleBackgroundFrames(MaleNewsAnchor));
                break;
            case "malenewsanchor2":
                animationCoroutine = StartCoroutine(CycleBackgroundFrames(MaleNewsAnchor2));
                break;
            case "jobletter":
                animationCoroutine = StartCoroutine(CycleBackgroundFrames(jobLetter));
                break;
            default:
                backgroundImage.sprite = null;
                break;
        }
    }

    private IEnumerator CycleBackgroundFrames(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0)
            yield break;

        int index = 0;
        while (true)
        {
            backgroundImage.sprite = frames[index];
            index = (index + 1) % frames.Length;
            yield return new WaitForSeconds(frameInterval);
        }
    }

    private IEnumerator NextScene()
    {
        EventManager.StopMusic?.Invoke();
        EventManager.FadeOut?.Invoke();
        yield return new WaitForSeconds(2f);

        Destroy(currentTextBox);
        currentTextBox = null;

        yield return new WaitForSeconds(2f);
        EventManager.NextScene?.Invoke();
    }

    [Serializable]
    private class Wrapper
    {
        public List<Entry> newsCasterIntro;
    }

    [Serializable]
    private class Line
    {
        public string speaker;
        public string text;
    }

    [Serializable]
    private class Entry
    {
        public int day;
        public Line[] lines;
    }
}
