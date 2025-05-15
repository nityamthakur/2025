using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToolSelection : MonoBehaviour
{
    [SerializeField] private Sprite defaultImg;
    [SerializeField] private Sprite selectionImg;
    private GameManager gameManager;
    private SelectedToolManager selectedToolManager; 
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager is not found in the scene.");
            return;
        }
        selectedToolManager = FindFirstObjectByType<SelectedToolManager>();
        if (selectedToolManager == null)
        {
            Debug.LogError("GameManager is not found in the scene.");
            return;
        }
        if (defaultImg == null)
        {
            Debug.LogError("Default Image component is not found on the ToolSelection object.");
            return;
        }
        if (selectionImg == null)
        {
            Debug.LogError("Selection Image component is not found on the ToolSelection object.");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        Debug.Log("Tool selected: " + gameObject.name);

        selectedToolManager.SelectTool(gameObject);

        // Play the sound for selecting the tool
        EventManager.PlaySound?.Invoke("switch1", true);
    }

    public void SelectToolEffect()
    {
        gameObject.GetComponent<Image>().sprite = selectionImg;
    }
    public void DeselectToolEffect()
    {
        gameObject.GetComponent<Image>().sprite = defaultImg;
    }
}
