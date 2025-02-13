using UnityEditor.Experimental.GraphView;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] SceneManager sceneManager;
    private string[] censorTargetWords;
    private string[] banTargetWords;

    private int totalCensorTargets = 0;
    private int currentCensorNum = 0;
    private int numCensorMistakes = 0;
    private int currentDay = 1;
    private bool dayEnded = false;
    private JobDetails jobDetails;
    private JobScene jobScene;
    // Set total score minimum to 0

    private int totalScore = 0;
    public int TotalScore
    {
        get { return totalScore; }
        private set { totalScore = Mathf.Max(0, value); }
    }


    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {    
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        jobDetails = new JobDetails();
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
        if (banWords.Length > 0) 
        {
            Debug.Log("Player mistakenly accepted the object.");
        }
        else if (currentCensorNum == totalCensorTargets && numCensorMistakes == 0) 
        {
            Debug.Log("Player has censored all targets!");
        }
        else
        {
            Debug.Log("Player has failed to censor correctly.");
        }
        jobDetails.numMediaProcessed += 1;

        EvaluatePlayerScore();
        ResetCensorTracking();
        CheckDayEnd();
    }

    public void EvalutatePlayerDestroy(string[] banWords)
    {
        bool playerSucceeds = false;
        
        if (banTargetWords.Length > 0)
        {
            foreach (string ban in banWords)
            {
                if (banTargetWords.Contains(ban))
                {
                    Debug.Log("Player correctly destroyed the object.");
                    playerSucceeds = true;
                }
                else
                {
                    Debug.Log("Player mistakenly destroyed the object.");
                }
            }
        }
        else
        {
            Debug.Log("Player mistakenly destroyed the object.");
        }
        jobDetails.numMediaProcessed += 1;

        TotalScore += playerSucceeds ? 2 : -2;

        EvaluatePlayerScore();
        ResetCensorTracking();
        CheckDayEnd();
    }

    public void EvaluatePlayerScore()
    {
        Debug.Log($"Results: \n{currentCensorNum}/{totalCensorTargets} words correctly censored. {numCensorMistakes} words incorrectly censored.");
        TotalScore += currentCensorNum - numCensorMistakes - (totalCensorTargets - currentCensorNum);
        Debug.Log($"Total Score: {TotalScore}");
    }

    public void CheckDayEnd()
    {
        if (jobDetails.numMediaProcessed >= jobDetails.numMediaNeeded)
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
        jobDetails.currClockTime = 0;
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