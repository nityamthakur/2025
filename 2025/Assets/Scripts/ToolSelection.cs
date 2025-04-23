using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToolSelection : MonoBehaviour
{
    [SerializeField] private Image toolImage;
    private SelectedToolManager selectedToolManager; 
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectedToolManager = GetComponentInParent<SelectedToolManager>();
        if (selectedToolManager == null)
        {
            Debug.LogError("SelectedToolManager is not found in the parent object.");
            return;
        }
        if (toolImage == null)
        {
            Debug.LogError("Image component is not found on the ToolSelection object.");
            return;
        }
        // Ensure the corresponding tool image is active
        toolImage.gameObject.SetActive(true);
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
        EventManager.PlaySound?.Invoke("switch1");

        SelectToolEffect();
    }

    public void SelectToolEffect()
    {
        toolImage.color = Color.yellow;
    }
    public void DeselectToolEffect()
    {
        toolImage.color = Color.white;
    }
}
