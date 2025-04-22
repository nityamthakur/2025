using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectedToolManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> tools;
    [SerializeField] private int[] toolAppearanceOrder;
    private GameObject selectedTool = null;
    private GameManager gameManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        
        // Add the available tools to the list
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
                tools.Add(child.gameObject);
        }

        // Check the game manager for the current tool, otherwise set the first tool as the selected tool
        selectedTool = tools.FirstOrDefault(obj => obj.name == gameManager.GetCurrentTool());
        if (selectedTool == null && tools.Count > 0)
        {
            selectedTool = tools[0];
            gameManager.SetCurrentTool(selectedTool.name);
        }
        else if (selectedTool == null)
        {
            Debug.LogError("No Tools found.");
            return;
        }

        selectedTool.GetComponent<ToolSelection>().SelectToolEffect();

        InitializeToolSelection();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitializeToolSelection()
    {
        int i = 0;
        foreach (GameObject tool in tools)
        {
            if (gameManager.gameData.GetCurrentDay() >= toolAppearanceOrder[i])
            {
                tool.SetActive(true);
            }
            else
            {
                tool.SetActive(false);
            }
            i += 1;
        }
        
    }

    public void SelectTool(GameObject tool)
    {
        if (!tools.Contains(tool)) 
            Debug.LogError("Selected tool is not in the tools array.");
        
        if (selectedTool != null)
            selectedTool.GetComponent<ToolSelection>().DeselectToolEffect();
        
        selectedTool = tool;
        gameManager.SetCurrentTool(tool.name);
    }
    public string GetSelectedTool()
    {
        return selectedTool.name;
    }
}
