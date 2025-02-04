using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor.Search;

public class SceneManager : MonoBehaviour
{
    [SerializeField] private GameObject UITextBox;
    [SerializeField] private Sprite FemaleNewsAnchor;
    [SerializeField] private Sprite MaleNewsAnchor;
    private GameObject currentTextBox;
    private TextMeshProUGUI TextBox;
    private Image backgroundImage;
    
    private int dayCounter = 0;
    private int linePos = 0;
    private string[] currentLines;

    void Start()
    {
        // Comment out to skip to job
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

        ShowBackgroundImage();
        ShowDialogTextBox();
        LoadJsonFromFile();
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
    }

    private void ShowDialogTextBox()
    {
        TextBox = currentTextBox.transform.Find("TextBox")?.GetComponent<TextMeshProUGUI>();
        if (TextBox == null)
        {
            Debug.LogError("Failed to find TextMeshProUGUI component.");
            return;
        }

        Button nextButton = currentTextBox.transform.Find("NextTextButton")?.GetComponent<Button>();
        if (nextButton == null)
        {
            Debug.LogError("Failed to find Button component.");
            return;
        }
        nextButton.onClick.AddListener(ReadNextLine);
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
            DestroyTextBox();
        }
    }

    private void DestroyTextBox()
    {
        if (currentTextBox != null)
        {
            Destroy(currentTextBox);
            currentTextBox = null;
        }
        BeginWorkDay();
    }

    private void BeginWorkDay()
    {
        
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
