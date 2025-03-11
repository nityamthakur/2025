using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.IO;

public class DayEndScene : MonoBehaviour
{
    [SerializeField] private GameObject dayEndObjectPrefab;
    [SerializeField] private Sprite newMericaEndingMap;
    [SerializeField] private Sprite newMericaEndingStar;
    [SerializeField] private Sprite greenPartyEnding;
    [SerializeField] private Sprite badEnding;
    private GameObject currentPrefab;
    private Image backgroundImage, textBoxBackground;
    private Button gameButton, nextButton;
    private TextMeshProUGUI buttonText, gameText, textBoxText;
    private GameManager gameManager;

    private int linePos = 0;
    private Line[] currentLines;

    public void Initialize()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void LoadDayEnd()
    {
        currentPrefab = Instantiate(dayEndObjectPrefab);
        if(currentPrefab == null)
        {
            Debug.LogError("currentPrefab is null");
            return;
        }

        SetUpDayEnd();
        EventManager.FadeIn?.Invoke();
    }

    public void SetUpDayEnd()
    {
        backgroundImage = currentPrefab.transform.Find("BackgroundImage").GetComponent<Image>();
        if(backgroundImage == null)
        {
            Debug.Log("Failed to find BackgroundImage component");
            return;
        }
        backgroundImage.color = Color.black;

        gameText = currentPrefab.transform.Find("GameText").GetComponent<TextMeshProUGUI>();
        if(gameText == null)
        {
            Debug.Log("Failed to find GameText component");
            return;
        }

        textBoxBackground = currentPrefab.transform.Find("TextBoxBackground").GetComponent<Image>();
        if(textBoxBackground == null)
        {
            Debug.Log("Failed to find TextBoxBackground component");
            return;
        }
        textBoxBackground.gameObject.SetActive(false);

        textBoxText = currentPrefab.transform.Find("TextBox").GetComponent<TextMeshProUGUI>();
        if(textBoxText == null)
        {
            Debug.Log("Failed to find TextBox component");
            return;
        }
        textBoxText.gameObject.SetActive(false);

        int currMoney = gameManager.gameData.GetCurrentMoney();
        int rentDue = gameManager.gameData.rent;
        gameText.text = $"Results Screen:<size=60%>\n\nMoney: ${currMoney}\n\nRent: - ${rentDue}\n\n New Total: ${currMoney - rentDue}";

        gameButton = currentPrefab.transform.Find("GameButton").GetComponent<Button>();
        if(gameButton == null)
        {
            Debug.Log("Failed to find GameButton component");
            return;
        }
        gameButton.onClick.AddListener(() => 
        {
            EventManager.PlaySound?.Invoke("switch1"); 
            gameButton.gameObject.SetActive(false);
            if(CheckGameOver())
                StartGameOver();

            else
                StartCoroutine(NextScene());
        });

        buttonText = gameButton.transform.Find("GameButtonText").GetComponent<TextMeshProUGUI>();
        if(buttonText == null)
        {
            Debug.Log("Failed to find GameButton component");
            return;
        }
        buttonText.text = "Continue";

        nextButton = currentPrefab.transform.Find("NextTextButton").GetComponent<Button>();
        if (nextButton == null)
        {
            Debug.LogError("Failed to find nextButton component.");
            return;
        }
        nextButton.onClick.AddListener(() => 
        {
            EventManager.PlaySound?.Invoke("switch1"); 
            ReadNextLine();
        });
        nextButton.gameObject.SetActive(false);
        
    }

    private bool CheckGameOver()
    {
        int rentDue = (gameManager.gameData.rent += 2) - 2;
        gameManager.gameData.SetCurrentMoney(gameManager.gameData.money - rentDue);

        if(SelectedEnding() > 0)
            return true;
        else
            return false;
    }

    private void StartGameOver()
    {
        gameText.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(true);
        textBoxBackground.gameObject.SetActive(true);
        textBoxText.gameObject.SetActive(true);
        LoadJsonFromFile();
    }

    private int SelectedEnding()
    {
        // Bad / Evicted Ending
        if(gameManager.gameData.money < 0)
        {
            EventManager.PlayMusic?.Invoke("darkfog");
            return 3;
        }
        // newMericaEnding
        else if(gameManager.gameData.newMericaRep >= gameManager.gameData.greenPartyRep && gameManager.gameData.day == 5)
        {
            EventManager.PlayMusic?.Invoke("americana");
            return 1;
        }
        // Green Party Ending
        else if(gameManager.gameData.newMericaRep >= gameManager.gameData.greenPartyRep && gameManager.gameData.day == 5)
        {
            return 2;
        }
        // No GameOver
        return 0;
    }

    private IEnumerator NextScene()
    {
        gameManager.SetCurrentDay(gameManager.gameData.day + 1);

        EventManager.StopMusic?.Invoke();
        EventManager.FadeOut?.Invoke();
        yield return new WaitForSeconds(2f);

        Destroy(currentPrefab);
        currentPrefab = null;
        
        yield return new WaitForSeconds(2f);
        EventManager.NextScene?.Invoke();
    }

    private IEnumerator RestartGame()
    {
        Debug.Log("Ending Game");

        EventManager.StopMusic?.Invoke();
        EventManager.FadeOut?.Invoke();
        yield return new WaitForSeconds(3f);
        
        Destroy(currentPrefab);
        currentPrefab = null;
        
        yield return new WaitForSeconds(1f);
        Time.timeScale = 1;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

        if (jsonObject != null && jsonObject.gameEndings.Count > 0)
        {
            currentLines = GetLinesForEnding(jsonObject.gameEndings, SelectedEnding());
            
            linePos = 0;
            ReadNextLine();
        }
        else
        {
            Debug.LogError("JSON parsing failed or empty list.");
        }
    }

    private Line[] GetLinesForEnding(List<Entry> entries, int ending)
    {
        foreach (var entry in entries)
        {
            if (entry.ending == ending)
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
            textBoxText.text = currentLine.text;
            ChangeSpeaker(currentLine);
            linePos++;
        }
        else
        {
            nextButton.interactable = false;
            StartCoroutine(RestartGame());
        }
    }

    private void ChangeSpeaker(Line currentLine)
    {
        backgroundImage.color = Color.white;

        // Change the speaker image based on who is speaking
        switch (currentLine.speaker.ToLower())
        {
            case "newmericaendingmap":
                backgroundImage.sprite = newMericaEndingMap;
                break;
            case "newmericaendingstar":
                backgroundImage.sprite = newMericaEndingStar;
                break;
            case "greenpartyending":
                backgroundImage.sprite = greenPartyEnding;
                break;
            case "badending":
                backgroundImage.sprite = badEnding;
                break;
            default:
                backgroundImage.sprite = null;
                backgroundImage.color = Color.black;
                break;
        }
    }

    [Serializable]
    private class Wrapper
    {
        public List<Entry> gameEndings;
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
        public int ending;
        public Line[] lines;
    }
}