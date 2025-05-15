using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CuttingTarget : MonoBehaviour
{
    [SerializeField] private string replacementText = "defaultReplacementText";
    private GameManager gameManager;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick() 
    {
        Debug.Log("Cutting target clicked: " + gameObject.name);

        if (!gameManager.IsCuttingModeActive()) return;
        
        gameManager.UpdateCensorTargets(replacementText);
        
    }
}
