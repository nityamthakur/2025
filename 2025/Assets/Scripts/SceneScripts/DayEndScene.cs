using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class DayEndScene : MonoBehaviour
{
    [SerializeField] private GameObject dayEndObjectPrefab;
    [SerializeField] private Sprite newMericaEndingMap;
    [SerializeField] private Sprite newMericaEndingStar;
    [SerializeField] private Sprite femaleNewsAnchor;
    [SerializeField] private Sprite maleNewsAnchor;
    [SerializeField] private Sprite badEnding;
    private GameObject currentPrefab;
    private Image backgroundImage, textBoxBackground;
    private Button gameButton, nextButton;
    private TextMeshProUGUI buttonText, fundsText, suppliesText, textBoxText, dayText;
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
        if (currentPrefab == null)
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
        if (backgroundImage == null)
        {
            Debug.Log("Failed to find BackgroundImage component");
            return;
        }
        backgroundImage.gameObject.SetActive(false);

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
            dayText.text = $"Day {gameManager.gameData.day}\nTime to head home";


        gameButton = currentPrefab.transform.Find("GameButton").GetComponent<Button>();
        if (gameButton == null)
        {
            Debug.Log("Failed to find GameButton component");
            return;
        }

        // Modified to handle transition to shop
        gameButton.onClick.AddListener(() =>
        {
            EventManager.PlaySound?.Invoke("switch1", true);
            gameButton.gameObject.SetActive(false);

            if (CheckGameOver())
            {
                StartGameOver();
            }
            else
            {
                // Go to shop after rent screen
                StartCoroutine(TransitionToShop());
            }
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
        int rentDue = gameManager.gameData.rent;

        fundsText.text = "Funds\n";
        fundsText.text += $"\n<align=left>Savings<line-height=0>\n<align=right>{currMoney}<line-height=1em>";
        fundsText.text += $"\n<align=left>Rent<line-height=0>\n<align=right>-{rentDue}<line-height=1em>";
        fundsText.text += $"\n\n<align=left>New Total<line-height=0>\n<align=right>${currMoney - rentDue}<line-height=1em>";
        
        suppliesText.text = $"Office supplies\n";
        suppliesText.text += "\nGetting Low On Pens";
        suppliesText.text += "\nRan out of batteries";
        suppliesText.text += "\nGetting Low On Stamp Ink";
    }

    private bool CheckGameOver()
    {
        int rentDue = (gameManager.gameData.rent += 2) - 2;
        gameManager.gameData.SetCurrentMoney(gameManager.gameData.money - rentDue);

        if (SelectedEnding() > 0)
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
        nextButton.gameObject.SetActive(true);
        textBoxBackground.gameObject.SetActive(true);
        textBoxText.gameObject.SetActive(true);
        LoadJsonFromFile();
    }

    private int SelectedEnding()
    {
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
                return 1;
            }
            // Neutral Ending
            else if (gameManager.gameData.PerformanceScale >= 0.33f)
            {
                return 2;
            }
            // Green Party (Bad) Ending
            else
            {
                return 3;
            }
        }
        // No GameOver
        return 0;
    }

    // New method to transition to the shop "scene"
    private IEnumerator TransitionToShop()
    {
        // Fade out the current scene
        EventManager.FadeOut?.Invoke();
        yield return new WaitForSeconds(2f);

        // Clean up the current day end UI
        Destroy(currentPrefab);
        currentPrefab = null;

        yield return new WaitForSeconds(1f);

        // Trigger the shop "scene" to appear
        EventManager.GoToShop?.Invoke(gameManager.gameData.money);
    }

    // This method would be called from the ShopManager when the player is done with the shop
    public static void TransitionFromShop()
    {
        // Find the game manager using the newer non-deprecated method
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            // Increment the day
            gameManager.SetCurrentDay(gameManager.gameData.day + 1);

            // Go to the next day's scene
            EventManager.StopMusic?.Invoke();
            EventManager.NextScene?.Invoke();
        }
        else
        {
            Debug.LogError("Could not find GameManager when transitioning from shop");
        }
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