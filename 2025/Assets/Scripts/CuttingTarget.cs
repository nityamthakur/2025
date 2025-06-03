using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CuttingTarget : MonoBehaviour
{
    [SerializeField] private string replacementText = "defaultReplacementText";
    [SerializeField] private TMP_Text buttonText;
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

    public void SetReplacementText(string text)
    {
        replacementText = text;
        buttonText.text = text;
    }
    public string GetReplacementText()
    {
        return replacementText;
    }

    public void OnClick() 
    {
        Debug.Log("Cutting target clicked: " + gameObject.name);

        if (!gameManager.IsCuttingModeActive()) return;
        gameManager.DecrementKnifeSlider();
        
        if (gameManager.GetCurrentCuttingRecipient().IsReplaceTarget())
        {
            gameManager.ReplaceTargetEnabled();
        }
        else
        {
            gameManager.NonReplaceTargetEnabled();
        }
        
        gameManager.UpdateCensorTargets(replacementText);
        
    }
}
