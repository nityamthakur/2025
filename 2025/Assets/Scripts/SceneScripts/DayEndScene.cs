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
    [SerializeField] private Sprite femaleNewsAnchor;
    [SerializeField] private Sprite maleNewsAnchor;
    [SerializeField] private Sprite badEnding, playtestQRCode;
    private GameObject currentPrefab;
    private Image backgroundImage, textBoxBackground, textOutlines, ratingImage;
    private Button gameButton, nextButton;
    private TextMeshProUGUI buttonText, fundsText, suppliesText, textBoxText, dayText;
    private GameManager gameManager;
    private int linePos = 0;
    public int gameRating = 0;
    private Line[] currentLines;

    public void Initialize()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void LoadDayEnd()
    {
        currentPrefab = Instantiate(dayEndObjectPrefab);
        if (currentPrefab == null)
        {
            Debug.LogError("currentPrefab is null");
            return;
        }
        Canvas prefabCanvas = currentPrefab.GetComponentInChildren<Canvas>();
        if (prefabCanvas != null)
        {
            prefabCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            prefabCanvas.worldCamera = Camera.main;
        }

        SetUpDayEnd();
        EventManager.FadeIn?.Invoke();
    }

    public void SetUpDayEnd()
    {
        backgroundImage = currentPrefab.transform.Find("BackgroundImage").GetComponent<Image>();
        if (backgroundImage == null)
        {
            Debug.Log("Failed to find BackgroundImage component");
            return;
        }
        backgroundImage.gameObject.SetActive(false);

        ratingImage = currentPrefab.transform.Find("RatingImage").GetComponent<Image>();
        if (ratingImage == null)
        {
            Debug.Log("Failed to find RatingImage component");
            return;
        }

        fundsText = currentPrefab.transform.Find("FundsText").GetComponent<TextMeshProUGUI>();
        if (fundsText == null)
        {
            Debug.Log("Failed to find FundsText component");
            return;
        }
        suppliesText = currentPrefab.transform.Find("SuppliesText").GetComponent<TextMeshProUGUI>();
        if (suppliesText == null)
        {
            Debug.Log("Failed to find SuppliesText component");
            return;
        }
        textOutlines = currentPrefab.transform.Find("TextOutlines").GetComponent<Image>();
        if (textOutlines == null)
        {
            Debug.Log("Failed to find TextOutlines component");
            return;
        }
        SetDayEndTextBoxes();

        textBoxBackground = currentPrefab.transform.Find("TextBoxBackground").GetComponent<Image>();
        if (textBoxBackground == null)
        {
            Debug.Log("Failed to find TextBoxBackground component");
            return;
        }
        textBoxBackground.gameObject.SetActive(false);

        textBoxText = currentPrefab.transform.Find("TextBox").GetComponent<TextMeshProUGUI>();
        if (textBoxText == null)
        {
            Debug.Log("Failed to find TextBox component");
            return;
        }
        textBoxText.gameObject.SetActive(false);

        dayText = currentPrefab.transform.Find("DayText").GetComponent<TextMeshProUGUI>();
        if (dayText == null)
        {
            Debug.LogError("Failed to find dayText component.");
            return;
        }
        else
            dayText.text = $"DAY {gameManager.gameData.day}\nTIME TO HEAD HOME";


        gameButton = currentPrefab.transform.Find("GameButton").GetComponent<Button>();
        if (gameButton == null)
        {
            Debug.Log("Failed to find GameButton component");
            return;
        }

        gameButton.onClick.AddListener(() =>
        {
            EventManager.PlaySound?.Invoke("switch1", true);
            gameButton.gameObject.SetActive(false);

            if (CheckGameOver())
                StartGameOver();
            else
                StartCoroutine(NextScene());
        });

        buttonText = gameButton.transform.Find("GameButtonText").GetComponent<TextMeshProUGUI>();
        if (buttonText == null)
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
            EventManager.PlaySound?.Invoke("switch1", true);
            ReadNextLine();
        });
        nextButton.gameObject.SetActive(false);
    }

    private void SetDayEndTextBoxes()
    {
        //Example
        //<align=left>This is the left aligned text<line-height=0>
        //<align=right>5,000<line-height=1em>
        //<align=left>This is the left aligned text<line-height=0>
        //<align=right>2,500<line-height=1em>
        int currMoney = gameManager.gameData.GetCurrentMoney();
        int jobMoney = gameManager.gameData.lastJobPay;
        int rentDue = gameManager.gameData.rent;

        fundsText.text = "Funds\n";
        fundsText.text += $"\n<align=left>Savings<line-height=0>\n<align=right>{currMoney - jobMoney}<line-height=1em>";
        fundsText.text += $"\n<align=left>Job Pay<line-height=0>\n<align=right>{jobMoney}<line-height=1em>";
        fundsText.text += $"\n<align=left>Rent<line-height=0>\n<align=right>-{rentDue}<line-height=1em>";

        foreach (var record in gameManager.gameData.dailyItemPurchases)
        {
            string itemName = record.Key;
            int itemCost = record.Value;
            currMoney -= itemCost;
            fundsText.text += $"\n<align=left>{itemName}<line-height=0>\n<align=right>-{itemCost}<line-height=1em>";
        }
        gameManager.gameData.dailyItemPurchases.Clear();

        fundsText.text += $"\n\n<align=left>New Total<line-height=0>\n<align=right>${currMoney - rentDue}<line-height=1em>";
        
        suppliesText.text = $"Office supplies\n";
        suppliesText.text += "\n<s>Getting Low On Pens";
        suppliesText.text += "\n<s>Ran out of batteries";
        suppliesText.text += "\n<s>Getting Low On Stamp Ink";
    }

    private bool CheckGameOver()
    {
        int selectedEnding = SelectedEnding();
        if (selectedEnding > 0)
        {
            dayText.gameObject.SetActive(false);
            return true;
        }
        else
            return false;
    }

    private void StartGameOver()
    {
        fundsText.gameObject.SetActive(false);
        suppliesText.gameObject.SetActive(false);
        textOutlines.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(true);
        textBoxBackground.gameObject.SetActive(true);
        textBoxText.gameObject.SetActive(true);
        LoadJsonFromFile();
    }

    private int SelectedEnding()
    {
        gameManager.gameData.SetCurrentMoney(-gameManager.gameData.rent, false);
        gameManager.gameData.IncreaseRent();

        // Bad / Evicted Ending
        if (gameManager.gameData.money < 0)
        {
            EventManager.PlayMusic?.Invoke("darkfog");
            return 3;
        }
        // Decide endings based on performance
        else if (gameManager.gameData.day == 5)
        {
            // NewMerica Ending
            if (gameManager.gameData.PerformanceScale >= 0.66f)
            {
                EventManager.PlayMusic?.Invoke("americana");
                return 1;
            }
            // Neutral Ending
            else if (gameManager.gameData.PerformanceScale >= 0.33f)
            {
                EventManager.PlayMusic?.Invoke("americana");
                return 2;
            }
            // Green Party (Bad) Ending
            else
            {
                EventManager.PlayMusic?.Invoke("darkfog");
                return 3;
            }
        }

        // No GameOver
        return 0;
    }

    private IEnumerator NextScene()
    {
        EventManager.StopMusic?.Invoke();
        EventManager.FadeOut?.Invoke();
        yield return new WaitForSeconds(2f);

        Destroy(currentPrefab);
        currentPrefab = null;

        gameManager.SetCurrentDay(gameManager.gameData.day + 1);
        SaveSystem.SaveGame(gameManager.gameData.saveSlot, gameManager.gameData);
        EventManager.SaveIconBlink?.Invoke(3f);

        yield return new WaitForSeconds(3f);
        EventManager.NextScene?.Invoke();
    }

    private IEnumerator RestartGame()
    {
        Debug.Log("Ending Game");
        AnalyticsManager.Instance.GameOver(gameManager.gameData, gameRating);

        EventManager.StopMusic?.Invoke();
        EventManager.FadeOut?.Invoke();
        yield return new WaitForSeconds(3f);

        Destroy(currentPrefab);
        currentPrefab = null;

        yield return new WaitForSeconds(1f);
        Time.timeScale = 1;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void RateGame()
    {
        EventManager.StopMusic?.Invoke();

        int ratingCount = 5;
        List<Button> buttons = new();

        backgroundImage.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        textBoxText.text = "<align=center>Thank You For Playing!\n\nPlease Give Our Game A Rating Out Of 5 Golden Hands</align>";

        gameButton.onClick.RemoveAllListeners();
        gameButton.onClick.AddListener(() => ShowPlaytestFormQRCode() );

        ratingImage.gameObject.SetActive(true);
        Button ratingButtonPrefab = ratingImage.transform.Find("Rating").GetComponent<Button>();
        ratingButtonPrefab.onClick.AddListener(() => 
        {
            gameRating = 1;
            RatingButtonUpdater(buttons);
        });
        buttons.Add(ratingButtonPrefab);

        Color color = ratingButtonPrefab.image.color;
        color.a = 0.2f;
        ratingButtonPrefab.image.color = color;

        for(int i = 1; i < ratingCount; i++)
        {
            int newRating = i + 1;
            Button ratingButton = Instantiate(ratingButtonPrefab, ratingImage.transform);
            ratingButton.onClick.AddListener(() => 
            {
                gameRating = newRating;
                RatingButtonUpdater(buttons);
            });
            buttons.Add(ratingButton);
        }
    }

    private void RatingButtonUpdater(List<Button> buttons)
    {
        gameButton.gameObject.SetActive(true);

        int num = 1;
        foreach(Button button in buttons)
        {
            Color color = button.image.color;
            if (num <= gameRating)
                color.a = 1f; // 100% opacity
            else
                color.a = 0.2f; // 20% opacity

            button.image.color = color; // Reassign the whole color back to the image
            num++;
        }
    }

    private void ShowPlaytestFormQRCode()
    {
        StartCoroutine(RestartGame());
        /* Removed for demo and future release
        gameButton.onClick.RemoveAllListeners();
        gameButton.onClick.AddListener(() => StartCoroutine(RestartGame()) );  

        ratingImage.gameObject.SetActive(false);
        backgroundImage.gameObject.SetActive(true);
        backgroundImage.sprite = playtestQRCode;
        textBoxText.text = "<align=center>Please Fill Out Our Playtest Form</align>";   
        */
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
            RateGame();
            //StartCoroutine(RestartGame());
        }
    }

    private void ChangeSpeaker(Line currentLine)
    {
        backgroundImage.gameObject.SetActive(true);
        // Change the speaker image based on who is speaking
        switch (currentLine.speaker.ToLower())
        {
            case "newmericaendingmap":
                backgroundImage.sprite = newMericaEndingMap;
                break;
            case "newmericaendingstar":
                backgroundImage.sprite = newMericaEndingStar;
                break;
            case "femalenewsanchor":
                backgroundImage.sprite = femaleNewsAnchor;
                break;
            case "malenewsanchor":
                backgroundImage.sprite = maleNewsAnchor;
                break;
            case "badending":
                backgroundImage.sprite = badEnding;
                break;
            default:
                backgroundImage.gameObject.SetActive(false);
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