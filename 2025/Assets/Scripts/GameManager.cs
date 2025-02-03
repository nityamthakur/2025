using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private string[] censorTargetWords;
    [SerializeField] private string[] banTargetWords;

    private int totalCensorTargets = 0;
    private int currentCensorNum = 0;
    
    private int totalScore = 0; // Keeps track of total correct censorships
    private int totalWordsProcessed = 0; // Keeps track of all words processed for scoring

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public string[] getCensorTargetWords()
    {
        return censorTargetWords;
    }

    public void setCensorTargetWords(string[] words)
    {
        censorTargetWords = words;
    }

    public string[] getBanTargetWords()
    {
        return banTargetWords;
    }

    public void setBanTargetWords(string[] words)
    {
        banTargetWords = words;
    }

    public void RegisterCensorTarget()
    {
        totalCensorTargets++;
        totalWordsProcessed++;
    }

    public void CensorTargetClicked()
    {
        currentCensorNum++;
        totalScore++;
        Debug.Log($"Censored Word! Score: {totalScore}/{totalWordsProcessed}");
    }

    public void ResetCensorTracking()
    {
        currentCensorNum = 0;
        totalCensorTargets = 0;
    }

    public void EvaluatePlayerAccept()
    {
        if (banTargetWords.Length > 0) 
        {
            Debug.Log("Player mistakenly accepted the object.");
        }
        else if (currentCensorNum == totalCensorTargets) 
        {
            Debug.Log("Player has censored all targets!");
        }
        else
        {
            Debug.Log("Player has not censored all targets.");
        }
        EvaluatePlayerScore();
        ResetCensorTracking();
    }

    public void EvalutatePlayerDestroy()
    {
        if (banTargetWords.Length > 0)
        {
            Debug.Log("Player correctly destroyed the object.");
        }
        else
        {
            Debug.Log("Player mistakenly destroyed the object.");
        }
        EvaluatePlayerScore();
        ResetCensorTracking();
    }

    public void EvaluatePlayerScore()
    {
        Debug.Log($"Final Score: {totalScore}/{totalWordsProcessed} words correctly censored.");
    }
}