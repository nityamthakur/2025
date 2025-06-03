using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameManager Instance { get; private set; }
    public static bool IsRestarting { get; private set; }
    public static event Action OnGameRestart;
    public bool gameZoomPaused = false;

    [SerializeField] SceneChanger sceneChanger;
    [SerializeField] ObjectSpawner objectSpawner;
    [SerializeField] AccessibilityManager accessibilityManager;

    [SerializeField] TextMeshProUGUI onScreenTimer;
    [SerializeField] GameObject onScreenDayChanger;

    [SerializeField] GameObject toolOverlayObj;
    [SerializeField] GameObject phoneOverlayObj;
    [SerializeField] GameObject cuttingTargetObj;
    [SerializeField] GameObject UVLightObj;
    [SerializeField] GameObject banStampObj;

    // Tool usage sliders
    [SerializeField] Slider banStampSlider;
    [SerializeField] Slider penSlider;
    [SerializeField] Slider lightSlider;
    [SerializeField] Slider knifeSlider;

    private JobDetails jobDetails;
    private JobScene jobScene;
    private Coroutine jobTimerCoroutine;
    private GameObject currentMediaObject;
    private SelectedToolManager selectedToolManager;

    private string[] censorTargetWords;
    private string[] banTargetWords;
    private string[][] replaceTargetWords;
    private int totalCensorTargets = 0;
    private int currentCensorNum = 0;
    private int numCensorMistakes = 0;
    private int totalReplaceTargets = 0;
    private int currentReplaceNum = 0;
    private int numReplaceMistakes = 0;
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
    public Camera mainCamera;
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

    public string[][] GetReplaceTargetWords()
    {
        return replaceTargetWords;
    }

    public void SetReplaceTargetWords(string[][] words)
    {
        replaceTargetWords = words;

        foreach (string[] wordSet in words)
        {
            foreach (string word in wordSet)
            {
                Debug.Log($"Replace target word: {word}");
            }
        }
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
    public GameObject GetCuttingTargetObj()
    {
        return cuttingTargetObj;
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

    public int GetBanSliderValue()
    {
        if (banStampSlider == null)
        {
            Debug.LogError("BanStampSlider is null.");
            return 0;
        }

        return (int)banStampSlider.value;
    }
    public void SetBanSliderMax(int value)
    {
        if (banStampSlider == null)
        {
            Debug.LogError("BanStampSlider is null.");
            return;
        }

        banStampSlider.maxValue = value;
    }
    public void GetPenSliderValue(float value)
    {
        if (penSlider == null)
        {
            Debug.LogError("PenSlider is null.");
            return;
        }

        penSlider.value = value;
    }
    public void SetPenSliderMax(float value)
    {
        if (penSlider == null)
        {
            Debug.LogError("PenSlider is null.");
            return;
        }

        penSlider.maxValue = value;
    }
    public void GetLightSliderValue(float value)
    {
        if (lightSlider == null)
        {
            Debug.LogError("LightSlider is null.");
            return;
        }

        lightSlider.value = value;
    }
    public void SetLightSliderMax(float value)
    {
        if (lightSlider == null)
        {
            Debug.LogError("LightSlider is null.");
            return;
        }

        lightSlider.maxValue = value;
    }
    public void GetKnifeSliderValue(float value)
    {
        if (knifeSlider == null)
        {
            Debug.LogError("KnifeSlider is null.");
            return;
        }

        knifeSlider.value = value;
    }
    public void SetKnifeSliderMax(float value)
    {
        if (knifeSlider == null)
        {
            Debug.LogError("KnifeSlider is null.");
            return;
        }

        knifeSlider.maxValue = value;
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
        Screen.fullScreen = PlayerPrefs.GetInt("FullScreenState", 0) == 1;
        // Set GameDevLoadDay if not absent to prevent errors on first install
        if (!PlayerPrefs.HasKey("GameDevLoadDay"))
            PlayerPrefs.SetInt("GameDevLoadDay", -1);

        // Set GameDevLoadDay if not absent to prevent errors on first install
        if (!PlayerPrefs.HasKey("FullScreenState"))
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

        if ((Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.Return)) && onScreenDayChanger.activeSelf)
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

    public IEnumerator UpdateArticleTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (jobDetails != null)
                jobDetails.articleClockTime += 1f;
            if (gameZoomPaused)
            {
                //Debug.Log("Time Stopped for UpdateArticleTime");
                yield return new WaitUntil(() => gameZoomPaused == false);
                //Debug.Log("Time Resumed for UpdateArticleTime");
            }
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

    public void RegisterReplaceTarget()
    {
        totalReplaceTargets++;
    }

    public void ReplaceTargetEnabled()
    {
        currentReplaceNum++;
        Debug.Log($"Replaced Word! Score: {currentReplaceNum}/{totalReplaceTargets}");
    }

    public void ReplaceTargetDisabled()
    {
        currentReplaceNum--;
        Debug.Log($"Unreplaced Word! Score: {currentReplaceNum}/{totalReplaceTargets}");
    }

    public void NonReplaceTargetEnabled()
    {
        numReplaceMistakes++;
        Debug.Log($"Incorrectly Replaced Word! Total Mistakes: {numReplaceMistakes}");
    }
    public void NonReplaceTargetDisabled()
    {
        numReplaceMistakes--;
        Debug.Log($"Unreplaced Word! Total Mistakes: {numReplaceMistakes}");
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

    public void IncrementBanSlider()
    {
        if (banStampSlider == null)
        {
            Debug.LogError("BanStampSlider is null.");
            return;
        }

        banStampSlider.value += 1;
    }
    public void DecrementBanSlider()
    {
        if (banStampSlider == null)
        {
            Debug.LogError("BanStampSlider is null.");
            return;
        }

        banStampSlider.value -= 1;
    }
    public void IncrementPenSlider()
    {
        if (penSlider == null)
        {
            Debug.LogError("PenSlider is null.");
            return;
        }

        penSlider.value += 1;
    }
    public void DecrementPenSlider()
    {
        if (penSlider == null)
        {
            Debug.LogError("PenSlider is null.");
            return;
        }

        penSlider.value -= 1;
    }
    public void IncrementLightSlider()
    {
        if (lightSlider == null)
        {
            Debug.LogError("LightSlider is null.");
            return;
        }

        lightSlider.value += 1;
    }
    public void DecrementLightSlider()
    {
        if (lightSlider == null)
        {
            Debug.LogError("LightSlider is null.");
            return;
        }

        lightSlider.value -= 1;
    }
    public void IncrementKnifeSlider()
    {
        if (knifeSlider == null)
        {
            Debug.LogError("KnifeSlider is null.");
            return;
        }

        knifeSlider.value += 1;
    }
    public void DecrementKnifeSlider()
    {
        if (knifeSlider == null)
        {
            Debug.LogError("KnifeSlider is null.");
            return;
        }

        knifeSlider.value -= 1;
    }

    public void UpdateCensorTargets(string replacementText)
    {
        currentCuttingRecipient.ToggleIsCut();

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
        currentReplaceNum = 0;
        totalReplaceTargets = 0;
        numReplaceMistakes = 0;
        hiddenImageExists = false;
        hiddenImageFound = false;
        banStampPressed = false;

        currentMediaObject = null;
    }
    public void EvaluatePlayerAccept(string[] banWords, string mediaTitle)
    {
        jobDetails.numMediaProcessed += 1;
        jobScene.UpdateMediaProcessedText(jobDetails.numMediaProcessed);

        // Check if the player has used the ban stamp if there are any bannable offenses or used the stamp incorrectly
        bool banWordsCheck = (banWords.Length > 0 || hiddenImageExists) == banStampPressed;

        bool hiddenImageCheck;
        if ((hiddenImageExists == hiddenImageFound) && (hiddenImageFound == banStampPressed))
            hiddenImageCheck = true;
        else if (!hiddenImageExists && banStampPressed)
            hiddenImageCheck = banWordsCheck;
        else
            hiddenImageCheck = false;

        // Check if player made any mistakes while censoring
        bool noCensorMistakes = (currentCensorNum == totalCensorTargets) && (numCensorMistakes == 0);

        // Check if player made any mistakes while replacing words
        bool noReplacementMistakes = (currentReplaceNum == totalReplaceTargets) && (numReplaceMistakes == 0);

        bool playerSucceeds = banWordsCheck && hiddenImageCheck && noCensorMistakes && noReplacementMistakes;
        //if (playerSucceeds && (banStampPressed || ((currentCensorNum == totalCensorTargets) && (numCensorMistakes == 0) && (currentReplaceNum == totalReplaceTargets) && (numReplaceMistakes == 0)))
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

        int mediaScore = 5;
        float maxPossibleScore = (float)mediaScore;

        // Lose 3 if article needed to be banned
        if (!banWordsCheck || !hiddenImageCheck)
            mediaScore -= 3;

        // Penalize for censoring mistakes made
        int totalMistakes = totalCensorTargets - currentCensorNum + numCensorMistakes + totalReplaceTargets - currentReplaceNum + numReplaceMistakes;
        int mistakeBuffer = totalMistakes > 0 ? 1 : 0;
        mediaScore -= Math.Abs(totalMistakes / jobDetails.penalty) + mistakeBuffer;

        // Change value on performance scale
        float performanceChange = maxPossibleScore - mediaScore > 0 ? -0.03f : 0.03f;
        gameData.PerformanceScale += performanceChange;

        // Score gets cut in half for money earned after timer is up
        if (jobDetails.currClockTime <= 0 && mediaScore > 0)
            mediaScore /= 2;

        TotalScore += Math.Clamp(mediaScore, 0, 5);

        EvaluatePlayerScore();
        ArticleAnalysisUpdate(mediaScore, mediaTitle, banWords, playerSucceeds, jobDetails.currClockTime <= 0);
        ResetPuzzleTracking();
        CheckDayEnd();
    }

    public void EvalutatePlayerDestroy(string[] banWords, string mediaTitle)
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
        if (jobDetails.currClockTime <= 0 && mediaScore > 0)
            mediaScore /= 2;

        Debug.Log($"mediaScore: {mediaScore}, TotalScore: {totalScore}");
        TotalScore += mediaScore;

        EvaluatePlayerScore();
        ArticleAnalysisUpdate(mediaScore, mediaTitle, banWords, playerSucceeds, jobDetails.currClockTime <= 0);
        ResetPuzzleTracking();
        CheckDayEnd();
    }

    public void EvaluatePlayerScore()
    {
        Debug.Log($"Total Score: {TotalScore}");
        Debug.Log($"Results: \n{currentCensorNum}/{totalCensorTargets} words correctly censored. {numCensorMistakes} words incorrectly censored.");

        Debug.Log($"Performance Scale: {gameData.PerformanceScale}");
    }

    public void ArticleAnalysisUpdate(int moneyEarned, string mediaTitle, string[] banwords, bool playerSucceeds, bool overTime)
    {
        // Find the matching media article and update its information

        Media foundMedia = gameData.releasedArticles.Find(media => media.title == mediaTitle);
        if (foundMedia == null)
        {
            Debug.Log($"Unable to find media with title: {mediaTitle}");
            return;
        }

        foundMedia.OverTime = overTime;
        foundMedia.moneyEarned = moneyEarned;
        foundMedia.timeSpent = jobDetails.articleClockTime;

        foundMedia.hiddenImageExists = hiddenImageExists;
        foundMedia.hiddenImageFound = hiddenImageFound;
        foundMedia.bannedWords = banwords;
        foundMedia.articleBanned = banStampPressed;
        foundMedia.numCensorableWords = totalCensorTargets;
        foundMedia.numCensoredCorrectly = currentCensorNum;
        foundMedia.numCensorMistakes = numCensorMistakes;
        foundMedia.noMistakes = playerSucceeds;
        //foundMedia.Print();
        jobDetails.articleClockTime = 0f;
    }

    public void CheckDayEnd()
    {
        // If the clock timer is 0 but player still has not met numMediaNeeded job is not over
        // Allows for players to do extra work if they are quick via the numMediaExtra
        if (jobDetails.currClockTime <= 0 && jobDetails.numMediaProcessed >= jobDetails.numMediaNeeded || jobDetails.numMediaProcessed >= jobDetails.numMediaExtra)
        {
            dayEnded = true;
            gameData.SetCurrentMoney(totalScore, false);
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
        StopCoroutine(UpdateArticleTime());
        jobDetails.currClockTime = 0f;
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
        StartCoroutine(UpdateArticleTime());

        jobDetails.currClockTime = time;
        jobDetails.articleClockTime = 0;

        float totalWorkTime = time; // Store total work time for calculations
        JobScene jobScene = GetJobScene();
        EventManager.StartClockMovement?.Invoke(time);

        while (jobDetails.currClockTime > 0)
        {
            jobDetails.currClockTime -= Time.deltaTime;
            if (gameZoomPaused)
            {
                Debug.Log("Time Stopped for BeginWorkTimer");
                yield return new WaitUntil(() => gameZoomPaused == false);
                Debug.Log("Resume Time for BeginWorkTimer");
            }
            yield return null;
        }

        EventManager.DisplayLightsOutImage?.Invoke(true);
        EventManager.PlaySound?.Invoke("switchoff", true);
        EventManager.StopMusic?.Invoke();
        jobDetails.currClockTime = 0;
    }

    private void SetOnScreenTimer()
    {
        onScreenTimer.text = $"Timer: {jobDetails.currClockTime:F2}s";
    }
}

[Serializable]
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
    public float articleClockTime;
    public int numMediaCorrect = 0;
    public JobDetails()
    {
        numMediaProcessed = 0;
        numMediaNeeded = 5;
        numMediaExtra = 5;
        penalty = 2;
        currClockTime = 0f;
        articleClockTime = 0f;
    }
}

[Serializable]
public class Media
{
    public string title;
    public string publisher;
    public string body;
    public string date;
    public int moneyEarned;
    public bool OverTime;

    public int day = 0; // Day the article was shown
    public float timeSpent = 0f;
    public bool noMistakes = true; // If the article was passed with no mistakes

    public bool hiddenImageExists = false;
    public bool hiddenImageFound = false;

    public string[] bannedWords = null; // Banned words
    public bool articleBanned = false; // If the article was banned

    public string[] censorWords = null; // Censored words
    public int numCensorableWords; // Number of words on the article that should be censored
    public int numCensoredCorrectly; // Number of words censored correctly
    public int numCensorMistakes; // Number of words incorrectly censored

    public bool CheckBanMistake()
    {
        return (bannedWords.Length > 0) == articleBanned;
    }

    public bool CheckHiddenImageMistake()
    {
        if ((hiddenImageExists == hiddenImageFound) && (hiddenImageFound == articleBanned))
            return true;
        else if (!hiddenImageExists && articleBanned)
            return CheckBanMistake();
        else
            return false; 
    }

    public bool CheckCensorMistake()
    {
        return (numCensoredCorrectly == numCensorableWords) && (numCensorMistakes == 0);
    }

    public int MaxPotentialMoney()
    {
        return bannedWords.Length == 0 ? 5 : 3;
    }

    public void Print()
    {
        Debug.Log($"MediaDetails: \nDay: {day} \ntimeSpent: {timeSpent} \nnoMistakes: {noMistakes} \nHiddenImageExist/Found: {hiddenImageExists}/{hiddenImageFound} \nArticleBanned: {articleBanned} \nbannedWords: {bannedWords} \nnumCensorableWords: {numCensorableWords} \nWordsCensoredCorrectly: {numCensoredCorrectly} \nnumCensorMistakes: {numCensorMistakes}");
    }
}