using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    [SerializeField] private GameObject UITextBox;
    private TextMeshProUGUI TextBox;
    private GameObject currentTextBox; // To track the created popup menu

    private int dayCounter = 1;
    private int linePos = 0;
    private string[] currentLines;

    void Start()
    {
        ShowDialogTextBox();
        LoadJsonFromFile();
    }

    void LoadJsonFromFile()
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

    void ParseJson(string json)
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

    string[] GetLinesForDay(List<Entry> entries, int day)
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

    public void ReadNextLine()
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

    public void ShowDialogTextBox()
    {
        if (currentTextBox != null) return;

        currentTextBox = Instantiate(UITextBox);
        TextBox = currentTextBox.transform.Find("TextBox").GetComponent<TextMeshProUGUI>();
        Button nextButton = currentTextBox.transform.Find("NextTextButton").GetComponent<Button>();
        nextButton.onClick.AddListener(() => { ReadNextLine(); });
    }

    private void DestroyTextBox()
    {
        if (currentTextBox != null)
        {
            Destroy(currentTextBox);
            currentTextBox = null;
        }
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
