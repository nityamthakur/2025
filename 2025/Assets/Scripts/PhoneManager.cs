using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PhoneManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI phoneText;
    [SerializeField] Transform scrollRectContent;
    [SerializeField] Button replacementButton;
    GameManager gameManager;

    public void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        foreach (Button button in scrollRectContent.GetComponents<Button>())
        {
            Destroy(button);
        }
    }

    public void SetPhoneText()
    {
        List<(string, int)> banWords = new(gameManager.GetBanTargetWords());
        List<(string, int)> censorWords = new(gameManager.GetCensorTargetWords());
        List<(string[] pair, int day)> replaceWords = new(gameManager.GetReplaceTargetWords());

        banWords.Sort();
        censorWords.Sort();
        //replaceWords.Sort();

        // Start with the Ban List
        phoneText.text = "<color=#FFFF00><b>BAN LIST:</color></b>\n";
        foreach ((string, int) phrase in banWords)
        {
            if (phrase.Item2 <= gameManager.gameData.GetCurrentDay())
                phoneText.text += phrase.Item1 + "\n\n";
        }

        // Only show the Censor List from Day 2 onward
        if (gameManager.gameData.GetCurrentDay() > 1)
        {
            phoneText.text += "<color=#FFFF00><b>CENSOR LIST:</color></b>\n";
            foreach ((string, int) phrase in censorWords)
            {
                // Prevent confusion for the player if a word appears on the banlist 
                // on later days, it wont show up on censor list anymore
                if (phrase.Item2 <= gameManager.gameData.GetCurrentDay() && !phoneText.text.Contains(phrase.Item1))
                    phoneText.text += phrase.Item1 + "\n\n";
            }
        }

        // Only show the Censor List from Day 4 onward
        if (gameManager.gameData.GetCurrentDay() > 3)
        {
            phoneText.text += "<color=#FFFF00><b>REPLACE LIST:</color></b>\n";
            foreach ((string[], int) phrase in replaceWords)
            {
                Button cuttingTarget = Instantiate(replacementButton, scrollRectContent);
                cuttingTarget.GetComponent<CuttingTarget>().SetReplacementText(phrase.Item1[1]);
            }
        }
    }
}
