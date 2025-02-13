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
    [SerializeField] private Sprite FemaleNewsAnchor;
    [SerializeField] private Sprite MaleNewsAnchor;
    private GameObject currentTextBox;
    private TextMeshProUGUI TextBox;
    private Button nextButton;
    private Image backgroundImage;    
    //private int currDay = 0;
    private int linePos = 0;
    private string[] currentLines;

    public void LoadDayStart(int day) {
        
        currentTextBox = Instantiate(UITextBox);

        if (currentTextBox == null)
        {
            Debug.LogError("currentTextBox is null.");
            return;
        }

        SetUpDayStart();
        EventManager.FadeIn?.Invoke(); 
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

        nextButton = currentTextBox.transform.Find("NextTextButton").GetComponent<Button>();
        if (nextButton == null)
        {
            Debug.LogError("Failed to find Button component.");
            return;
        }
        nextButton.onClick.AddListener(ReadNextLine);

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
            currentLines = GetLinesForDay(jsonObject.newsCasterIntro, GameManager.Instance.GetCurrentDay());
            
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
            StartCoroutine(NextScene());
        }
    }

    private IEnumerator NextScene()
    {
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
    private class Entry
    {
        public int day;
        public string[] lines;
    }
}
