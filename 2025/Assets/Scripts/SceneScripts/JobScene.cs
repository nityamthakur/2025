using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using TMPro;

public class JobScene : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private ObjectSpawner objectSpawner;
    [SerializeField] private GameObject jobScenePrefab;
    [SerializeField] private Sprite workBackgroundImage;
    private GameObject currJobScene;
    private Image backgroundImage;
    private Button startWorkButton;
    private TextMeshProUGUI screenText;
    
    // ---------------------------------
    [SerializeField] private GameObject jobBuildingPrefab;
    [SerializeField] private Sprite jobBuildingImage;
    private GameObject outsideBuildingObject;
    
    // ---------------------------------
    public JobDetails jobDetails;
    private int currDay = 0;

    // ---------------------------------

    public void LoadJobStart(int day) {

        ShowBuildingTransition();
        SetUpJobStart(day);
    }

    private void ShowBuildingTransition()
    {
        Debug.Log("Starting Building Transition");
        outsideBuildingObject = Instantiate(jobBuildingPrefab);
        if (outsideBuildingObject == null)
        {
            Debug.LogError("outsideBuildingObject object is null in ShowBuildingTransition.");
            return;
        }

        backgroundImage = outsideBuildingObject.transform.Find("BackgroundImage").GetComponent<Image>();
        if(backgroundImage == null)
        {
            Debug.Log("Failed to find BackgroundImage in ShowBuildingTransition.");
            return;
        }
        backgroundImage.sprite = jobBuildingImage; 

        StartCoroutine(TransitionBuildingFade());
    }

    private IEnumerator TransitionBuildingFade()
    {
        EventManager.FadeIn?.Invoke();
        yield return new WaitForSeconds(2f);

        // Press to Continue or Continue Automatically?
        yield return new WaitForSeconds(2f);

        EventManager.FadeOut?.Invoke();
        yield return new WaitForSeconds(2f);
        Destroy(outsideBuildingObject);
        outsideBuildingObject = null;
        yield return new WaitForSeconds(2f);
        EventManager.FadeIn?.Invoke(); 
    }

    private void SetUpJobStart(int day) {
        Debug.Log("Setting up Job Start");

        currDay = day;

        currJobScene = Instantiate(jobScenePrefab);

        if (currJobScene == null)
        {
            Debug.LogError("currJobScene object is null in LoadJobStart of JobScene class.");
            return;
        }

        Canvas canvas = currJobScene.GetComponentInChildren<Canvas>();
        if (canvas == null && mainCamera == null)
        {
            Debug.LogError("Failed to set Event Camera. Canvas or mainCamera is missing.");
        }
        else
        {
            canvas.worldCamera = mainCamera.GetComponent<Camera>();
        }


        backgroundImage = currJobScene.transform.Find("BackgroundImage").GetComponent<Image>();
        if(backgroundImage == null)
        {
            Debug.Log("Failed to find BackgroundImage in SetUpJobStart");
            return;
        }
        backgroundImage.sprite = workBackgroundImage; 

        startWorkButton = currJobScene.transform.Find("WorkButton").GetComponent<Button>();
        if (startWorkButton == null)
        {
            Debug.LogError("Failed to find startWorkButton component in SetUpJobStart.");
            return;
        }
        startWorkButton.onClick.AddListener(BeginWorkDay);

        screenText = currJobScene.transform.Find("ComputerScreenText").GetComponent<TextMeshProUGUI>();
        if (screenText == null)
        {
            Debug.LogError("Failed to find screenText component in ShowResults.");
            return;
        }
        screenText.text = "Censor List:\nBrown Oil Conglomerate\nBolivia\nBrian Jay\n\nBan List:\nNewMerica";
    }

    private void BeginWorkDay(){
        jobDetails = new JobDetails();
        objectSpawner.StartMediaSpawn(currDay, this);
        startWorkButton.gameObject.SetActive(false);
    }

    public void ShowResults() {
        TextMeshProUGUI buttonText = startWorkButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null) {
            buttonText.text = "End Day";
        } else {
            Debug.LogError("TextMeshProUGUI component not found on startWorkButton.");
        }

        startWorkButton.onClick.RemoveAllListeners();
        startWorkButton.onClick.AddListener(() => StartCoroutine(NextScene()));
        startWorkButton.gameObject.SetActive(true);
        screenText.text = "Day X Results:\n\nMedia Processed: 0\n\nSupervisors Notified of Your Day\n\nProfit: $10 + $ 5 (Bonus)\n\nPossiibility of Promotion: High";


        // Set the results text based on the job details
    }

    private IEnumerator NextScene()
    {
        EventManager.FadeOut?.Invoke();
        yield return new WaitForSeconds(2f);

        Destroy(currJobScene);
        currJobScene = null;
        jobDetails = null;
        
        yield return new WaitForSeconds(2f);
        EventManager.NextScene?.Invoke();
    }
}

public class JobDetails {
    // For using a media target goal for the day
    public int numMediaProcessed; 
    public int numMediaNeeded;

    // For using a time based system for the day
    public int currClockTime;
    public int clockTimeJobEnd;
    public JobDetails() {
        numMediaProcessed = 0;
        numMediaNeeded = 5;
        currClockTime = 0;
        clockTimeJobEnd = 1000;
    }
}
