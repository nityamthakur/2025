using System;
using System.Collections;
using TMPro;
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
    
    [SerializeField] GameObject UVLightObj;
    private JobDetails jobDetails;
    private JobScene jobScene;
    private Coroutine jobTimerCoroutine;

    private string[] censorTargetWords;
    private string[] banTargetWords;
    private int totalCensorTargets = 0;
    private int currentCensorNum = 0;
    private int numCensorMistakes = 0;
    private bool canCensor = false;
    private bool dayEnded = false;
    private bool toolOverlayCreated = false;
    private string currentTool = "CensorPen";
    private bool hiddenImageExists = false;
    private bool hiddenImageFound = false;

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

    public void SetToolFunctionality(bool canCensor)
    {
        switch (currentTool) 
        {
            case "CensorPen":
                this.canCensor = canCensor;
                break;
            case "UVLight":
                UVLightObj.SetActive(canCensor);
                break;
            default:
                Debug.LogError($"Invalid tool: {currentTool}");
                break;
        }
        
    }
    public bool CanCensor()
    {
        return canCensor;
    }
    public void SetToolOverlayCreated(bool created)
    {
        toolOverlayCreated = created;
    }
    public bool IsToolOverlayCreated()
    {
        return toolOverlayCreated;
    }
    public string GetCurrentTool()
    {
        return currentTool;
    }
    public void SetCurrentTool(string tool)
    {
        currentTool = tool;

        if (tool == "UVLight")
        {
            UVLightObj.SetActive(true);
        }
        else if (UVLightObj.activeSelf)
        {
            UVLightObj.SetActive(false);
        }
    }
    public GameObject GetUVLightObj()
    {
        return UVLightObj;
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

    // -------------------------------------
    // Functions
    public void RestartGame()
    {
        IsRestarting = true;
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

    public void ResetPuzzleTracking()
    {
        currentCensorNum = 0;
        totalCensorTargets = 0;
        numCensorMistakes = 0;
        hiddenImageExists = false;
        hiddenImageFound = false;
    }

    public void EvaluatePlayerAccept(string[] banWords)
    {
        bool playerSucceeds = banWords.Length == 0 && !hiddenImageExists;

        if (playerSucceeds && (currentCensorNum == totalCensorTargets) && (numCensorMistakes == 0))
        {
            EventManager.ShowCorrectBuzzer?.Invoke(true);
            EventManager.PlaySound?.Invoke("correctBuzz");
        }
        else
        {
            EventManager.ShowCorrectBuzzer?.Invoke(false);
            EventManager.PlaySound?.Invoke("errorBuzz");
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
            EventManager.PlaySound?.Invoke("correctBuzz");
        }
        else
        {
            EventManager.ShowCorrectBuzzer?.Invoke(false);
            EventManager.PlaySound?.Invoke("errorBuzz");
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
            ResetJobDetails();
            jobScene.ShowMediaProcessedText(false);
            toolOverlayCreated = false;
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