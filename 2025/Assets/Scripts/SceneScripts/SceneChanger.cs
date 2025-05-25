using UnityEngine;
using System;
using System.Collections.Generic;

public class SceneChanger : MonoBehaviour
{
    public SceneChanger Instance { get; private set; }
    [SerializeField] private GameManager gameManager;
    [SerializeField] private MainMenuScene mainMenuScene;
    [SerializeField] private DayStartScene dayStartScene;
    [SerializeField] private JobScene jobScene;
    [SerializeField] private DayEndScene dayEndScene;
    [SerializeField] private GameObject fadingScreenPrefab;
    private bool mainMenuDone = false;
    private int currentSceneIndex = 0;
    private List<Action> sceneSequence;

    public void Initialize()
    {
        if(gameManager == null)
        {
            Debug.Log("gameManager is null in SceneChanger");
        }
        // Define the order of the scenes
        sceneSequence = new List<Action>
        {
            () => dayStartScene.LoadDayStart(),
            () => jobScene.LoadJobStart(),
            () => dayEndScene.LoadDayEnd()
        };

        Instantiate(fadingScreenPrefab);
        dayStartScene.Initialize();
        jobScene.Initialize();
        dayEndScene.Initialize();
    }

    public void StartGame(int loadSlot)
    {
        if (loadSlot > 0)
        {
            PlayerPrefs.SetInt("LoadSlot", -1);
            EventManager.NextScene?.Invoke();

            // Continue Playtime counter
            StartCoroutine(gameManager.UpdatePlayTime());
        }
        else if (PlayerPrefs.GetInt("GameDevLoadDay") > 0)
        {
            gameManager.gameData.day = PlayerPrefs.GetInt("GameDevLoadDay");
            PlayerPrefs.SetInt("GameDevLoadDay", -1);
            EventManager.NextScene?.Invoke();
        }
        else
        {
            PlayerPrefs.SetInt("LoadSlot", -1);

            // Start Game
            // Comment out if using with debug
            //mainMenuScene.LoadMainMenu();

            // For Debugging
            // Change the starting day
            gameManager.gameData.day = 1;

            // Start the game at day end
            //currentSceneIndex = 3;
            //dayEndScene.LoadDayEnd();

            // Start the game at the job scene
            currentSceneIndex = 2;
            jobScene.LoadJobStart();
        }
    }

    public void StartNextScene()
    {
        if(!mainMenuDone)
        {
            // Start Playtime counter for first time
            StartCoroutine(gameManager.UpdatePlayTime());
        }

        mainMenuDone = true;
        // Call the function for the current scene
        sceneSequence[currentSceneIndex]?.Invoke();

        // Increment and loop back if at the end
        currentSceneIndex = (currentSceneIndex + 1) % sceneSequence.Count;
    }

    void OnEnable()
    {
        EventManager.NextScene += StartNextScene;
    }

    void OnDisable()
    {
        EventManager.NextScene -= StartNextScene;
    }
}
