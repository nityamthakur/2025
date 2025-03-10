using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DayEndScene : MonoBehaviour
{
    [SerializeField] private GameObject dayEndObjectPrefab;
    [SerializeField] private Sprite newMericaEndingMap;
    [SerializeField] private Sprite newMericaEndingStar;
    [SerializeField] private Sprite greenPartyEnding;
    [SerializeField] private Sprite badEnding;
    private GameObject currentPrefab;
    private Image backgroundImage;
    private Button gameButton;
    private TextMeshProUGUI buttonText, gameText;
    private bool isGameOver = false;
    
    private GameManager gameManager;

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
        CheckGameOver();

        backgroundImage = currentPrefab.transform.Find("BackgroundImage").GetComponent<Image>();
        if(backgroundImage == null)
        {
            Debug.Log("Failed to find BackgroundImage component");
            return;
        }

        gameText = currentPrefab.transform.Find("GameText").GetComponent<TextMeshProUGUI>();
        if(gameText == null)
        {
            Debug.Log("Failed to find GameText component");
            return;
        }
        gameText.text = $"Results Screen:<size=60%>\n\nMoney: ${gameManager.gameData.GetCurrentMoney()+10}\n\nRent: - $10\n\n New Total: ${gameManager.gameData.GetCurrentMoney()}";

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
            CheckForNewDayStart();
        });

        buttonText = gameButton.transform.Find("GameButtonText").GetComponent<TextMeshProUGUI>();
        if(buttonText == null)
        {
            Debug.Log("Failed to find GameButton component");
            return;
        }
        buttonText.text = "Continue";
        
    }

    private void ChangeBackgroundImage( Sprite sprite = null)
    {
        backgroundImage.sprite = sprite;
    }

    private void CheckForNewDayStart()
    {
        if(isGameOver)
        {
            gameText.text = "Game Over\n\n<size=50%>Couldn't pay rent";
            buttonText.text = "Return to Main Menu";

            gameButton.gameObject.SetActive(true);
            gameButton.onClick.RemoveAllListeners();
            gameButton.onClick.AddListener(()=>
            {
                gameButton.gameObject.SetActive(false);
                EventManager.PlaySound?.Invoke("switch1"); 
                StartCoroutine(StartGameOver());
            });
        }

        else
            StartCoroutine(NextScene());
    }


    private void CheckGameOver()
    {
        int rentDue = 10;
        gameManager.gameData.SetCurrentMoney(gameManager.gameData.money - rentDue);
        if(gameManager.gameData.money < 0)
            isGameOver = true;
        else
            isGameOver = false;
    }

    private IEnumerator NextScene()
    {
        EventManager.StopMusic?.Invoke();
        EventManager.FadeOut?.Invoke();
        yield return new WaitForSeconds(2f);

        Destroy(currentPrefab);
        currentPrefab = null;
        
        yield return new WaitForSeconds(2f);
        EventManager.NextScene?.Invoke();
    }

    private IEnumerator StartGameOver()
    {
        EventManager.StopMusic?.Invoke();
        EventManager.FadeOut?.Invoke();
        yield return new WaitForSeconds(3f);
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}