using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectedToolManager : MonoBehaviour
{
    [SerializeField] private GameObject toolsLocation;
    [SerializeField] private List<GameObject> tools;
    [SerializeField] private int[] toolAppearanceOrderByDay;
    private GameObject selectedTool = null;
    private bool canCensor = false;
    private bool canCut = false;
    private GameManager gameManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();

        if (toolsLocation == null)
        {
            Debug.LogError("Tools location is not assigned in the inspector.");
            return;
        }
        // Add the available tools to the list
        foreach (Transform child in toolsLocation.transform)
        {
            if (child.gameObject.activeSelf)
                tools.Add(child.gameObject);
        }

        // set the first tool as the selected tool
        if (selectedTool == null && tools.Count > 0)
        {
            selectedTool = tools[0];
        }
        else if (selectedTool == null)
        {
            Debug.LogError("No Tools found.");
            return;
        }

        selectedTool.GetComponent<ToolSelection>().SelectToolEffect();

        InitializeToolAppearance();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeToolAppearance()
    {
        int i = 0;
        foreach (GameObject tool in tools)
        {
            if (gameManager.gameData.GetCurrentDay() >= toolAppearanceOrderByDay[i])
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
        
        // Deselect the currently selected tool
        if (selectedTool != null)
        {
            selectedTool.GetComponent<ToolSelection>().DeselectToolEffect();

            // Exit cutting mode if the selected tool is CraftKnife
            if (selectedTool.name == "CraftKnife")
                gameManager.ExitCuttingMode();
        }

        // Select the new tool unless it's the same as the current one
        if (selectedTool == tool)
        {
            SetToolFunctionality(false);
            selectedTool = null;
        }
        else 
        {
            selectedTool = tool;
            SetToolFunctionality(true);
            selectedTool.GetComponent<ToolSelection>().SelectToolEffect();
        }
        
        
    }
    public string GetSelectedTool()
    {
        return selectedTool.name;
    }

    public void SetToolFunctionality(bool canUse)
    {
        if (selectedTool == null) return;

        switch (selectedTool.name) 
        {
            case "BanStamp":
                gameManager.BanStampObjActive(canUse);
                gameManager.UVLightObjActive(false);
                canCensor = false;
                canCut = false;
                break;
            case "CensorPen":
                canCensor = canUse;
                gameManager.BanStampObjActive(false);
                gameManager.UVLightObjActive(false);
                canCut = false;
                break;
            case "UVLight":
                gameManager.UVLightObjActive(canUse);
                gameManager.BanStampObjActive(false);
                canCensor = false;
                canCut = false;
                break;
            case "CraftKnife":
                canCut = canUse;
                gameManager.BanStampObjActive(false);
                gameManager.UVLightObjActive(false);
                canCensor = false;
                break;
            default:
                Debug.LogError($"Invalid tool: {selectedTool.name}");
                break;
        }
        
    }

    public bool CanCensor()
    {
        return canCensor;
    }
    public bool CanCut()
    {
        return canCut;
    }
}
