using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameManager Instance { get; private set; }
    public static bool IsRestarting { get; private set; }
    public static event Action OnGameRestart;

    [SerializeField] SceneChanger sceneChanger;
    [SerializeField] ObjectSpawner objectSpawner;
    [SerializeField] AccessibilityManager accessibilityManager;

    [SerializeField] TextMeshProUGUI onScreenTimer;
    [SerializeField] GameObject onScreenDayChanger;
    
    [SerializeField] GameObject toolOverlayObj;
    [SerializeField] GameObject phoneOverlayObj;
    [SerializeField] GameObject UVLightObj;
    [SerializeField] GameObject banStampObj;
    private JobDetails jobDetails;
    private JobScene jobScene;
    private Coroutine jobTimerCoroutine;
    private GameObject currentMediaObject;
    private SelectedToolManager selectedToolManager;

    private string[] censorTargetWords;
    private string[] banTargetWords;
    private int totalCensorTargets = 0;
    private int currentCensorNum = 0;
    private int numCensorMistakes = 0;
    private bool dayEnded = false;
    private bool hiddenImageExists = false;
    private bool hiddenImageFound = false;
    private bool banStampPressed = false;
    private bool cuttingModeActive = false;
    private CensorTarget currentCuttingRecipient = null;

    // Set total score minimum to 0
    private int totalScore = 0;
    public int TotalScore
    {
        get { return totalScore; }
        private set { totalScore = Mathf.Max(0, value); }
    }

    public GameData gameData;

    // --------------------------------------------
    // Getters and Setters
    public bool IsDayEnded()
    {
        return dayEnded;
    }

    public void SetCurrentDay(int day)
    {
        gameData.day = day;
        selectedToolManager.InitializeToolAppearance();
    }

    public JobDetails GetJobDetails()
    {
        return jobDetails;
    }

    public void SetJobScene(JobScene workScene)
    {
        if (workScene == null)
        {
            Debug.LogError("jobScene is null.");
        }

        jobScene = workScene;
        dayEnded = false;
    }

    public JobScene GetJobScene()
    {
        return jobScene;
    }

    public GameObject GetCurrentMediaObject()
    {
        return currentMediaObject;
    }
    public void SetCurrentMediaObject(GameObject mediaObj)
    {
        if (mediaObj == null)
        {
            Debug.LogError("mediaObj is null.");
            return;
        }

        currentMediaObject = mediaObj;
    }

    public string[] GetCensorTargetWords()
    {
        return censorTargetWords;
    }

    public void SetCensorTargetWords(string[] words)
    {
        censorTargetWords = words;
    }

    public string[] GetBanTargetWords()
    {
        return banTargetWords;
    }

    public void SetBanTargetWords(string[] words)
    {
        banTargetWords = words;
    }

    public GameObject GetToolOverlayObj()
    {
        return toolOverlayObj;
    }
    public void ToolOverlayObjActive(bool active)
    {
        if (toolOverlayObj == null)
        {
            Debug.LogError("ToolOverlayObj is null.");
            return;
        }

        toolOverlayObj.SetActive(active);
    }
    public GameObject GetPhoneOverlayObj()
    {
        return phoneOverlayObj;
    }

    public GameObject GetUVLightObj()
    {
        return UVLightObj;
    }
    public void UVLightObjActive(bool active)
    {
        if (UVLightObj == null)
        {
            Debug.LogError("UVLightObj is null.");
            return;
        }

        UVLightObj.SetActive(active);
    }
    public void SetUVLightTarget(GameObject target)
    {
        if (target == null)
        {
            Debug.LogError("Target is null.");
            return;
        }

        UVLightObj.GetComponent<UVLight>().SetTargetCollider(target.GetComponent<Collider2D>());
    }
    public void SetTargetExists(bool exists)
    {
        hiddenImageExists = exists;
    }

    public void SetTargetFound(bool found)
    {
        hiddenImageFound = found;
    }

    public bool UVLightTargetFound()
    {
        return hiddenImageFound;
    }

    public GameObject GetBanStampObj()
    {
        return banStampObj;
    }
    public void BanStampObjActive(bool active)
    {
        if (banStampObj == null)
        {
            Debug.LogError("BanStampObj is null.");
            return;
        }

        banStampObj.SetActive(active);
    }
    public void SetBanStampColliderActive(bool active)
    {
        if (banStampObj == null)
        {
            Debug.LogError("BanStampObj is null.");
            return;
        }

        banStampObj.GetComponent<BanStamp>().BanStampColliderActive(active);
    }

    public void SetBanStampPressed(bool pressed)
    {
        banStampPressed = pressed;
    }
    public bool IsBanStampPressed()
    {
        return banStampPressed;
    }
    public void SetBanStampColliderParentToMediaObj(GameObject banStampCollider) 
    {
        if (banStampCollider == null)
        {
            Debug.LogError("BanStampCollider is null.");
            return;
        }

        banStampCollider.transform.SetParent(currentMediaObject.transform);
    }
    public void ResetBanStampCollider(GameObject banStampCollider)
    {
        if (banStampCollider == null)
        {
            Debug.LogError("BanStampCollider is null.");
            return;
        }

        banStampCollider.transform.SetParent(banStampObj.transform);
        banStampCollider.transform.localPosition = Vector3.zero;
        banStampCollider.transform.localRotation = Quaternion.identity;
        banStampCollider.transform.localScale = Vector3.one;
    }

    public void SetCuttingModeActive(bool active)
    {
        cuttingModeActive = active;
    }
    public bool IsCuttingModeActive()
    {
        return cuttingModeActive;
    }
    public CensorTarget GetCurrentCuttingRecipient()
    {
        return currentCuttingRecipient;
    }

    // -------------------------------------
    // Functions
    public void RestartGame()
    {
        IsRestarting = true;
        currentMediaObject = null;
        OnGameRestart?.Invoke();
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        IsRestarting = false;
    }

    void Awake()
    {
        // Set GameDevLoadDay if not absent to prevent errors on first install
        if(!PlayerPrefs.HasKey("GameDevLoadDay"))
            PlayerPrefs.SetInt("GameDevLoadDay", -1);

        // Set GameDevLoadDay if not absent to prevent errors on first install
        if(!PlayerPrefs.HasKey("FullScreenState"))
            PlayerPrefs.SetInt("FullScreenState", 0);

        objectSpawner.Initialize();
        sceneChanger.Initialize();
        accessibilityManager.Initialize();
        sceneChanger.StartGame(CheckLoadGameSave());

        jobDetails = new JobDetails();
        onScreenTimer.enabled = false; // Hide the onscreen timer
        onScreenDayChanger.SetActive(false); // Hide the jump to day debug box

        selectedToolManager = FindFirstObjectByType<SelectedToolManager>();
        if (selectedToolManager == null)
        {
            Debug.LogError("GameManager is not found in the scene.");
            return;
        } 

        if (toolOverlayObj == null)
        {
            Debug.LogError("ToolOverlayObj is null.");
            return;
        }
        toolOverlayObj.transform.GetChild(0).gameObject.SetActive(false); // Hide the tool overlay object

        if (phoneOverlayObj == null)
        {
            Debug.LogError("PhoneOverlayObj is null.");
            return;
        }
        phoneOverlayObj.transform.GetChild(0).gameObject.SetActive(false); // Hide the phone overlay object
    }

    private int CheckLoadGameSave()
    {
        int loadSlot = PlayerPrefs.GetInt("LoadSlot");
        if (loadSlot > 0)
        {
            //Debug.Log($"Game was restarted or opened through load: {loadSlot}");
            gameData = new GameData(SaveSystem.LoadGame(loadSlot));
        }
        else
        {
            //Debug.Log($"Game was restarted or opened without load: {loadSlot}");
            gameData = new GameData();
        }
        return loadSlot;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            //Debug.Log("Showing Timer");
            //onScreenTimer.enabled = !onScreenTimer.enabled; // Hide the onscreen timer
        }

        if (onScreenTimer.enabled == true)
            SetOnScreenTimer();

        if (Input.GetKeyDown(KeyCode.D))
        {
            onScreenDayChanger.SetActive(!onScreenDayChanger.activeSelf);
        }

        if ( (Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.Return)) && onScreenDayChanger.activeSelf)
        {
            string text = onScreenDayChanger.GetComponent<TMP_InputField>().text;
            if (int.TryParse(text, out int num) && num > 0)
            {                
                PlayerPrefs.SetInt("GameDevLoadDay", num);
                RestartGame();
            }
            else
                Debug.Log("Enter a valid whole number, greater than 0.");
        }
    }

    public IEnumerator UpdatePlayTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            gameData.playTime += 1f;  // Increment playtime every second
        }
    }


    public void RegisterCensorTarget()
    {
        totalCensorTargets++;
    }

    public void CensorTargetEnabled()
    {
        currentCensorNum++;
        Debug.Log($"Censored Word! Score: {currentCensorNum}/{totalCensorTargets}");
    }

    public void CensorTargetDisabled()
    {
        currentCensorNum--;
        Debug.Log($"Uncensored Word! Score: {currentCensorNum}/{totalCensorTargets}");
    }

    public void NonCensorTargetEnabled()
    {
        numCensorMistakes++;
        Debug.Log($"Incorrectly Censored Word! Total Mistakes: {numCensorMistakes}");
    }
    public void NonCensorTargetDisabled()
    {
        numCensorMistakes--;
        Debug.Log($"Uncensored Word! Total Mistakes: {numCensorMistakes}");
    }

    public void EnterCuttingMode(CensorTarget recipient)
    {
        if (recipient == null)
        {
            Debug.LogError("Target is null.");
            return;
        }
        
        if (cuttingModeActive && currentCuttingRecipient != null)
        {
            currentCuttingRecipient.CuttingModeEffect(false);
        }
        currentCuttingRecipient = recipient;
        cuttingModeActive = true;

        Debug.Log("Cutting mode activated");
    }

    public void ExitCuttingMode()
    {
        if (currentCuttingRecipient != null)
        {
            currentCuttingRecipient.CuttingModeEffect(false);
            currentCuttingRecipient = null;
        }
        cuttingModeActive = false;

        Debug.Log("Cutting mode deactivated");
    } 

    public void UpdateCensorTargets(string replacementText)
    {
        currentCuttingRecipient.SetToIsCut();
        
        selectedToolManager.SetToolFunctionality(false);
        currentMediaObject.GetComponent<Entity>().UpdateCensorBoxes(currentCuttingRecipient, replacementText);
        selectedToolManager.SetToolFunctionality(true);

        ExitCuttingMode();
    }
       

    public void ResetPuzzleTracking()
    {
        currentCensorNum = 0;
        totalCensorTargets = 0;
        numCensorMistakes = 0;
        hiddenImageExists = false;
        hiddenImageFound = false;
        banStampPressed = false;

        currentMediaObject = null;
    }

    public void EvaluatePlayerAccept(string[] banWords)
    {
        bool playerSucceeds;
        
        // Check if the player has used the ban stamp if there are any bannable offenses or used the stamp incorrectly
        if (banWords.Length != 0 || (hiddenImageExists && hiddenImageFound))
            playerSucceeds = banStampPressed;
        else 
            playerSucceeds = !banStampPressed;
        
        //bool playerSucceeds = banWords.Length == 0 && !hiddenImageExists;

        if (playerSucceeds && (banStampPressed || ((currentCensorNum == totalCensorTargets) && (numCensorMistakes == 0))))
        {
            EventManager.ShowCorrectBuzzer?.Invoke(true);
            EventManager.PlaySound?.Invoke("correctBuzz", true);
        }
        else
        {
            EventManager.ShowCorrectBuzzer?.Invoke(false);
            EventManager.PlaySound?.Invoke("errorBuzz", true);
        }

        jobDetails.numMediaProcessed += 1;
        jobScene.UpdateMediaProcessedText(jobDetails.numMediaProcessed);

        int mediaScore = 5;
        float maxPossibleScore = (float) mediaScore;

        if (currentCensorNum == 0 && totalCensorTargets == 0) 
        {
            mediaScore = playerSucceeds ? 3 : 0;
            maxPossibleScore = 3f;
        } 
        
        // Penalize for mistakes made
        int totalMistakes = totalCensorTargets - currentCensorNum + numCensorMistakes;
        int mistakeBuffer = totalMistakes > 0 ? 1 : 0;
        mediaScore -= Math.Abs(totalMistakes / jobDetails.penalty) + mistakeBuffer;
        
        // Change value on performance scale
        float performanceChange = maxPossibleScore - mediaScore > 0 ? -0.03f : 0.03f;
        gameData.PerformanceScale += performanceChange; 

        // Score gets cut in half for money earned after timer is up
        if(jobDetails.currClockTime <= 0 && mediaScore > 0)
            mediaScore /= 2;

        TotalScore += Math.Clamp(mediaScore, 0, 5);

        EvaluatePlayerScore();
        ResetPuzzleTracking();
        CheckDayEnd();
    }

    public void EvalutatePlayerDestroy(string[] banWords)
    {
        bool playerSucceeds = banWords.Length != 0 || (hiddenImageExists && hiddenImageFound);

        if (playerSucceeds)
        {
            EventManager.ShowCorrectBuzzer?.Invoke(true);
            EventManager.PlaySound?.Invoke("correctBuzz", true);
        }
        else
        {
            EventManager.ShowCorrectBuzzer?.Invoke(false);
            EventManager.PlaySound?.Invoke("errorBuzz", true);
        }

        jobDetails.numMediaProcessed += 1;
        jobScene.UpdateMediaProcessedText(jobDetails.numMediaProcessed);
    
        int mediaScore = playerSucceeds ? 3 : 0;
        float maxPossibleScore = 3f;

        float performanceChange = maxPossibleScore - mediaScore > 0 ? -0.03f : 0.03f;
        gameData.PerformanceScale += performanceChange;

        // Score gets cut in half for money earned after timer is up
        if(jobDetails.currClockTime <= 0 && mediaScore > 0)
            mediaScore /= 2;
        
        Debug.Log($"mediaScore: {mediaScore}, TotalScore: {totalScore}");
        TotalScore += mediaScore;


        EvaluatePlayerScore();
        ResetPuzzleTracking();
        CheckDayEnd();
    }

    public void EvaluatePlayerScore()
    {
        Debug.Log($"Total Score: {TotalScore}");
        Debug.Log($"Results: \n{currentCensorNum}/{totalCensorTargets} words correctly censored. {numCensorMistakes} words incorrectly censored.");
    
        Debug.Log($"Performance Scale: {gameData.PerformanceScale}");
    }

    public void CheckDayEnd()
    {
        // If the clock timer is 0 but player still has not met numMediaNeeded job is not over
        // Allows for players to do extra work if they are quick via the numMediaExtra
        if (jobDetails.currClockTime <= 0 && jobDetails.numMediaProcessed >= jobDetails.numMediaNeeded || jobDetails.numMediaProcessed >= jobDetails.numMediaExtra)
        {
            dayEnded = true;
            jobScene.ShowResults(jobDetails.numMediaProcessed, TotalScore);
            TotalScore = 0;
            currentMediaObject = null;
            ResetJobDetails();
        }
    }

    // Reset job details for next day
    public void ResetJobDetails()
    {
        jobDetails.numMediaProcessed = 0;
        StopCoroutine(jobTimerCoroutine);
        jobDetails.currClockTime = 0;
    }

    public void StartJobTimer(float time)
    {
        if (jobTimerCoroutine != null)
        {
            StopCoroutine(jobTimerCoroutine); // Stops any previous job timer
        }
        jobTimerCoroutine = StartCoroutine(BeginWorkTimer(time));
    }

    private IEnumerator BeginWorkTimer(float time)
    {
        Debug.Log("Job Timer Started...");
        jobDetails.currClockTime = time;

        float totalWorkTime = time; // Store total work time for calculations
        JobScene jobScene = GetJobScene();

        while (jobDetails.currClockTime > 0)
        {
            jobDetails.currClockTime -= Time.deltaTime;

            if (jobScene != null)
            {
                float progress = 1f - (jobDetails.currClockTime / totalWorkTime);
                jobScene.UpdateClockHands(progress);
            }

            yield return null;
        }

        EventManager.ShowLightsOutImage?.Invoke();
        EventManager.PlaySound?.Invoke("switchoff", true);
        EventManager.StopMusic?.Invoke();
        jobDetails.currClockTime = 0;
    }

    private void SetOnScreenTimer()
    {
        onScreenTimer.text = $"Timer: {jobDetails.currClockTime:F2}s";
    }
}

public class JobDetails
{
    // For using a media target goal for the day
    public int numMediaProcessed;
    public int numMediaNeeded;
    public int numMediaExtra;
    // penalty represents mistakes made before getting less money 
    // ie, penalty = 1 => with 1 mistake   = -1 mediaScore
    //     penalty = 2 => with 2 mistakes  = -1 mediaScore
    //     penalty = 2 => with 3 mistakes  = -1 mediaScore
    public int penalty;

    // For using a time based system for the day
    public float currClockTime;
    public JobDetails()
    {
        numMediaProcessed = 0;
        numMediaNeeded = 5;
        numMediaExtra = 5;
        penalty = 2;
        currClockTime = 0f;
    }
}