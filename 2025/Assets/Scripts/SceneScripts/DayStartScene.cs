using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Collections;
using Unity.VisualScripting;
using System.Reflection;

public class DayStartScene : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject UITextBox;
    [SerializeField] private Sprite[] FemaleNewsAnchor, MaleNewsAnchor, MaleNewsAnchor2, apartment;
    [SerializeField] private Sprite rentLetter, rentNotice, jobLetter;
    private float frameInterval = 0.2f;
    private GameObject currentTextBox;
    private TextMeshProUGUI TextBox, dayText;
    private Button nextButton;
    private Image backgroundImage, textBoxBackground;    
    private int linePos = 0;
    private Line[] currentLines;
    private GameManager gameManager;
    private TypewriterText typewriterText;
    private Coroutine animationCoroutine;
    private Action delay;

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
        Canvas prefabCanvas = currentTextBox.GetComponentInChildren<Canvas>();
        if (prefabCanvas != null)
        {
            prefabCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            prefabCanvas.worldCamera = Camera.main;
        }

        SetUpDayStart();
        DayStartEventChecker();
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
        TextBox.AddComponent<TypewriterText>();
        typewriterText = TextBox.transform.GetComponent<TypewriterText>();

        textBoxBackground = currentTextBox.transform.Find("TextBoxBackground").GetComponent<Image>();
        if (textBoxBackground == null)
        {
            Debug.LogError("Failed to find TextBoxBackground component.");
            return;
        }

        dayText = currentTextBox.transform.Find("DayText").GetComponent<TextMeshProUGUI>();
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
            EventManager.PlaySound?.Invoke("switch1", true);
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
            //ReadNextLine();
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

            // Instantly show the text if currently writing
            if(typewriterText.MessageWriting())
                typewriterText.InstantMessage(currentLines[linePos - 1].text);
            else
            {
                // Run a timed delay based on JSON file text
                if (currentLine.speaker.ToLower() == "delay")
                {
                    CheckDelay(currentLine);
                }
                // Play a song or stop a song in between dialogue 
                else if (currentLine.speaker.ToLower() == "playmusic")
                {
                    EventManager.PlayMusic?.Invoke(currentLine.text);
                    linePos++;
                    ReadNextLine();
                }
                else if (currentLine.speaker.ToLower() == "stopmusic")
                {
                    EventManager.StopMusic?.Invoke();
                    linePos++;
                    ReadNextLine();
                }
                else if (currentLine.speaker.ToLower() == "playsound")
                {
                    EventManager.PlaySound?.Invoke(currentLine.text, true);
                    linePos++;
                    ReadNextLine();
                }
                else if (currentLine.speaker.ToLower() == "function")
                {
                    Invoke(currentLine.text, 0f);
                    linePos++;
                    ReadNextLine();            
                }
                else
                {
                    // Set the text in the dialogue box
                    typewriterText.TypewriteMessage(currentLine.text);
                    ChangeSpeaker(currentLine);
                    linePos++;

                    // If there is no text, proceed to the next line, for swapping just the image
                    if (currentLine.text == "")
                        ReadNextLine();
                }
            }
        }
        else
        {
            // Instantly show the text if currently writing
            if(typewriterText.MessageWriting())
                typewriterText.InstantMessage(currentLines[linePos - 1].text);
            else
            {
                nextButton.interactable = false;
                EventManager.DisplayMenuButton?.Invoke(false);
                StartCoroutine(NextScene());
            }
        }
    }

    private void ChangeSpeaker(Line currentLine)
    {
        // Change the speaker image based on who is speaking
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        backgroundImage.color = Color.white;
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
            case "apartment":
                animationCoroutine = StartCoroutine(CycleBackgroundFrames(apartment)); ;
                break;
            case "jobletter":
                backgroundImage.sprite = jobLetter;
                break;
            case "rentletter":
                backgroundImage.sprite = rentLetter;
                break;
            case "rentnotice":
                backgroundImage.sprite = rentNotice;
                break;
            default:
                backgroundImage.color = Color.black;
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

    private void CheckDelay(Line currentLine)
    {
        if (float.TryParse(currentLine.text, out float result))
        {
            TextBox.gameObject.SetActive(false);
            textBoxBackground.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);

            delay = () =>
            {
                TextBox.gameObject.SetActive(true);
                textBoxBackground.gameObject.SetActive(true);
                nextButton.gameObject.SetActive(true);
                linePos++;
                ReadNextLine();
            };

            StartCoroutine(StartDelay(result));
        }
    }

    private IEnumerator StartDelay(float time)
    {
        yield return new WaitForSeconds(time);
        delay?.Invoke();
    }

    private void DayStartEventChecker()
    {
        EventManager.FadeIn?.Invoke();
        EventManager.DisplayMenuButton?.Invoke(true);
        if (gameManager.gameData.GetCurrentDay() == 1)
        {
            StartCoroutine(DayStartOpening());
        }
        else
        {
            ReadNextLine();            
        }
    }

    public void DayOneStart()
    {
        StartCoroutine(DayStartOpening());
    }

    private IEnumerator DayStartOpening()
    {
        dayText.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        backgroundImage.color = Color.black;
        backgroundImage.sprite = null;
        typewriterText.TypewriteMessage("This game saves your progress automatically when the logo in the bottom right is flashing.");
        EventManager.SaveIconBlink?.Invoke(-1f);

        while (typewriterText.MessageWriting())
        {
            yield return null;
        }
        yield return new WaitForSeconds(5f);
        EventManager.SaveIconBlink?.Invoke(0f);

        ReadNextLine();
        dayText.gameObject.SetActive(true);
    }

    public void ShowDayText()
    {
        dayText.gameObject.SetActive(true);
    }
    public void HideDayText()
    {
        dayText.gameObject.SetActive(false);
    }

    private void StopAnimationCoroutines()
    {
        StopCoroutine(animationCoroutine);
    }

    private IEnumerator NextScene()
    {
        EventManager.StopMusic?.Invoke();
        EventManager.FadeOut?.Invoke();
        yield return new WaitForSeconds(2f);
        StopAnimationCoroutines();
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
