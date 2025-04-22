using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Toolbars;

public class DayStartScene : MonoBehaviour
{
    [SerializeField] private GameObject UITextBox;
    [SerializeField] private Sprite FemaleNewsAnchor, MaleNewsAnchor, MaleNewsAnchor2;
    [SerializeField] private Sprite jobLetter, rentLetter, rentNotice, apartment;
    private GameObject currentTextBox;
    private TextMeshProUGUI TextBox;
    private Button nextButton;
    private Image backgroundImage, textBoxBackground;    
    //private int currDay = 0;
    private int linePos = 0;
    private Line[] currentLines;

    private GameManager gameManager;
    private TypewriterText typewriterText;
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

        SetUpDayStart();
        
        if(!EventManager.IsMusicPlaying())
            EventManager.PlayMusic?.Invoke("menu");

        DayStartEventChecker();
    }

    private void SetUpDayStart() {
        backgroundImage = currentTextBox.transform.Find("BackgroundImage").GetComponent<Image>();
        if(backgroundImage == null)
        {
            Debug.Log("Failed to find Image component");
            return;
        }
        backgroundImage.sprite = FemaleNewsAnchor; 

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
                if(currentLine.speaker.ToLower() == "delay")
                {  
                    CheckDelay(currentLine);
                }
                // Play a song or stop a song in between dialogue 
                else if(currentLine.speaker.ToLower() == "playmusic")
                {
                    EventManager.PlayMusic?.Invoke(currentLine.text);
                    linePos++;
                    ReadNextLine();
                }
                else if(currentLine.speaker.ToLower() == "stopmusic")
                {
                    EventManager.StopMusic?.Invoke();
                    linePos++;
                    ReadNextLine();
                }
                else if(currentLine.speaker.ToLower() == "playsound")
                {
                    EventManager.PlaySound?.Invoke(currentLine.text);
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
                    if(currentLine.text == "")
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
        switch (currentLine.speaker.ToLower())
        {
            case "femalenewsanchor":
                backgroundImage.sprite = FemaleNewsAnchor;
                break;
            case "malenewsanchor":
                backgroundImage.sprite = MaleNewsAnchor;
                break;
            case "malenewsanchor2":
                backgroundImage.sprite = MaleNewsAnchor2;
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
            case "apartment":
                backgroundImage.sprite = apartment;
                break;
            default:
                backgroundImage.sprite = null;
                break;
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
        if(gameManager.gameData.GetCurrentDay() == 1)
        {
            StartCoroutine(DayStartOpening());
        }   
        else
        {
            EventManager.FadeIn?.Invoke();
            EventManager.DisplayMenuButton?.Invoke(true); 
            ReadNextLine();
        }
    }

    private IEnumerator DayStartOpening()
    {
        backgroundImage.sprite = rentLetter;
        TextBox.gameObject.SetActive(false);
        textBoxBackground.gameObject.SetActive(false);

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() =>
        {
            TextBox.gameObject.SetActive(true);
            textBoxBackground.gameObject.SetActive(true);
            EventManager.PlaySound?.Invoke("switch1");
            ReadNextLine();
        });

        yield return new WaitForSeconds(1f);
        EventManager.PlaySound?.Invoke("doorbell");
        yield return new WaitForSeconds(2f);
        EventManager.PlaySound?.Invoke("papercomein");
        yield return new WaitForSeconds(0.5f);
        EventManager.FadeIn?.Invoke();
        //EventManager.PlayMusic?.Invoke("Some Music For The Day Start / Apartment");
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
