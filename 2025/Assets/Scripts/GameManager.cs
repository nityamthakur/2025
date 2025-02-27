using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameManager Instance { get; private set; }
    [SerializeField] TextMeshProUGUI onScreenTimer;
    [SerializeField] GameObject performanceBuzzersObj;
    private PerformanceBuzzers performanceBuzzers;
    private JobDetails jobDetails;
    private JobScene jobScene;
    private Coroutine jobTimerCoroutine;

    private string[] censorTargetWords;
    private string[] banTargetWords;
    private int totalCensorTargets = 0;
    private int currentCensorNum = 0;
    private int numCensorMistakes = 0;
    private bool canCensor = false;

    private int currentDay = 1;
    private bool dayEnded = false;

    // Set total score minimum to 0
    private int totalScore = 0;
    public int TotalScore
    {
        get { return totalScore; }
        private set { totalScore = Mathf.Max(0, value); }
    }

    // --------------------------------------------
    // Getters and Setters
    public bool IsDayEnded()
    {
        return dayEnded;
    }

    public int GetCurrentDay()
    {
        return currentDay;
    }
    public void SetCurrentDay(int day)
    {
        currentDay = day;
    }

   
    public JobDetails GetJobDetails()
    {
        return jobDetails;
    }

    public void SetJobScene(JobScene workScene)
    {
        if(workScene == null) {
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

    public void SetCensorFunctionality(bool canCensor)
    {
        this.canCensor = canCensor;
    }
    public bool CanCensor()
    {
        return canCensor;
    }

    // -------------------------------------
    // Functions
    void Start()
    {
        jobDetails = new JobDetails();
        onScreenTimer.enabled = false; // Hide the onscreen timer

        if (performanceBuzzersObj == null)
        {
            Debug.LogError("PerformanceBuzzers is null in GameManager.");
        }
        else {
            performanceBuzzers = performanceBuzzersObj.GetComponent<PerformanceBuzzers>();
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            Debug.Log("Restarting Game");
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.T)) {
            Debug.Log("Showing Timer");
            onScreenTimer.enabled = !onScreenTimer.enabled; // Hide the onscreen timer
        }
        if(onScreenTimer.enabled == true)
            setOnScreenTimer();
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

    public void ResetCensorTracking()
    {
        currentCensorNum = 0;
        totalCensorTargets = 0;
        numCensorMistakes = 0;
    }

    public void EvaluatePlayerAccept(string[] banWords)
    {
        bool playerSucceeds = banWords.Length == 0;
        
        if (playerSucceeds && (currentCensorNum == totalCensorTargets) && (numCensorMistakes == 0)) 
            performanceBuzzers.ShowCorrectBuzzer();
        else
            performanceBuzzers.ShowIncorrectBuzzer();
        
        jobDetails.numMediaProcessed += 1;

        if (currentCensorNum == 0 && totalCensorTargets == 0)
            TotalScore += playerSucceeds ? 1 : -1;
        else
            TotalScore += currentCensorNum - numCensorMistakes - (totalCensorTargets - currentCensorNum);

        EvaluatePlayerScore();
        ResetCensorTracking();
        CheckDayEnd();
    }

    public void EvalutatePlayerDestroy(string[] banWords)
    {
        bool playerSucceeds = banWords.Length != 0;
        
        jobDetails.numMediaProcessed += 1;

        TotalScore += playerSucceeds ? 2 : -2;
        if (playerSucceeds) performanceBuzzers.ShowCorrectBuzzer();
        else performanceBuzzers.ShowIncorrectBuzzer();

        EvaluatePlayerScore();
        ResetCensorTracking();
        CheckDayEnd();
    }

    public void EvaluatePlayerScore()
    {
        Debug.Log($"Results: \n{currentCensorNum}/{totalCensorTargets} words correctly censored. {numCensorMistakes} words incorrectly censored.");
        
        Debug.Log($"Total Score: {TotalScore}");
    }

    public void CheckDayEnd()
    {
        if (jobDetails.numMediaProcessed >= jobDetails.numMediaNeeded || jobDetails.currClockTime <= 0)
        {
            Debug.Log("Day has ended.");
            dayEnded = true;
            jobScene.ShowResults(currentDay, jobDetails.numMediaProcessed, TotalScore);
            TotalScore = 0;

            ResetJobDetails();
            SetCurrentDay(currentDay + 1);
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
        JobScene jobScene = GetJobScene(); // Get reference to JobScene

        while (jobDetails.currClockTime > 0)
        {
            jobDetails.currClockTime -= Time.deltaTime;
            
            // Update the clock hands each frame
            if (jobScene != null)
            {
                float progress = 1f - (jobDetails.currClockTime / totalWorkTime); // 0 to 1
                jobScene.UpdateClockHands(progress);
            }

            yield return null; // Wait for next frame
        }

        jobDetails.currClockTime = 0;
    }


    private void setOnScreenTimer()
    {
        onScreenTimer.text = $"Timer: {jobDetails.currClockTime:F2}s";
    }
}

public class JobDetails {
    // For using a media target goal for the day
    public int numMediaProcessed; 
    public int numMediaNeeded;

    // For using a time based system for the day
    public float currClockTime;
    public JobDetails() {
        numMediaProcessed = 0;
        numMediaNeeded = 7;
        currClockTime = 0f;
    }
}