using UnityEngine;

public class AccessibilityManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPrefab;
    [SerializeField] private AudioManager audioManager;
    private GameObject pauseMenu;

    public void Initialize()
    {   
        audioManager.Initialize();
        pauseMenu = Instantiate(pauseMenuPrefab);
        if (pauseMenu == null)
            Debug.Log("Pause Menu Prefab is missing from AccessibilityManager.");
        else
        {
            OptionsMenu optionsMenu = pauseMenu.GetComponent<OptionsMenu>();
            if(optionsMenu == null)
                Debug.Log("OptionsMenu Component is missing from pauseMenuPrefab.");
        }
    }

    void Update()
    {
        // Check if the Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EventManager.OpenOptionsMenu?.Invoke();
            EventManager.OptionsChanger?.Invoke("");         
        }
    }

    public void TogglePauseMenu()
    {            
        EventManager.ReactivateMainMenuButtons?.Invoke(); 
        bool isActive = pauseMenu.activeSelf;
        Time.timeScale = isActive ? 1 : 0;
        pauseMenu.SetActive(!isActive);
    }

    void OnEnable()
    {
        EventManager.OpenOptionsMenu += TogglePauseMenu;
    }

    void OnDisable()
    {
        EventManager.OpenOptionsMenu -= TogglePauseMenu;
    }
}
