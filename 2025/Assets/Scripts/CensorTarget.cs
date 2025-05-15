using UnityEngine;
using UnityEngine.EventSystems;
using System;
using TMPro;

public class CensorTarget : MonoBehaviour, IPointerClickHandler
{
    private SpriteRenderer spriteRenderer;
    private bool isCensored = false;
    private bool isCensorTarget = false;
    private bool isCuttingMode = false;
    private bool isCut = false;
    private string originalText = "defaultOriginalText";
    private TMP_Text textCmp;
    private int textCmpFirstIndex;
    private int wordCharCount;
    private int lineIndex;
    private int wordIndex;
    private Vector3 topLeft;

    private GameManager gameManager;
    private SelectedToolManager selectedToolManager;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager = FindFirstObjectByType<GameManager>();
        selectedToolManager = FindFirstObjectByType<SelectedToolManager>();
    }

    public void SetToCensorTarget()
    {
        isCensorTarget = true;
    }
    public void SetToIsCut()
    {
        isCut = true;
    }

    public TMP_Text GetTextComponent()
    {
        return textCmp;
    }
    public void SetTextCmpFirstIndex(int textCmpFirstIndex)
    {
        this.textCmpFirstIndex = textCmpFirstIndex;
    }
    public int GetTextCmpFirstIndex()
    {
        return textCmpFirstIndex;
    }
    public int GetWordCharCount()
    {
        return wordCharCount;
    }
    public void SetWordCharCount(int wordCharCount)
    {
        this.wordCharCount = wordCharCount;
    }
    public (int, int) GetWordLocation()
    {
        return (lineIndex, wordIndex);
    }
    public Vector3 GetTopLeft()
    {
        return topLeft;
    }

    public void InitializeCensorTarget(string originalText, TMP_Text textCmp, int textCmpFirstIndex, int wordCharCount, int lineIndex, int wordIndex, Vector3 topLeft)
    {
        if (textCmp == null)
        {
            Debug.LogError("Text component is null.");
            return;
        }
        this.originalText = originalText;
        this.textCmp = textCmp;
        this.textCmpFirstIndex = textCmpFirstIndex;
        this.wordCharCount = wordCharCount;
        this.lineIndex = lineIndex;
        this.wordIndex = wordIndex;
        this.topLeft = topLeft;
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (selectedToolManager.CanCensor() && selectedToolManager.GetSelectedTool() == "CensorPen") 
            HandleCensorInteraction();
        else if (selectedToolManager.CanCut() && selectedToolManager.GetSelectedTool() == "CraftKnife") 
            HandleCutInteraction();
        
    }

    private void HandleCensorInteraction()
    {
        if (isCensored) 
        {
            spriteRenderer.enabled = false;
            isCensored = false;

            if (isCensorTarget)
            {
                gameManager.CensorTargetDisabled();
            }
            else 
            {
                gameManager.NonCensorTargetDisabled();
            }
        }
        else
        {
            spriteRenderer.enabled = true;
            isCensored = true;

            // Play the censor sound
            EventManager.PlaySound?.Invoke("censor", true);

            if (isCensorTarget)
            {
                gameManager.CensorTargetEnabled();
            }
            else 
            {
                gameManager.NonCensorTargetEnabled();
            }
        }
    }

    private void HandleCutInteraction()
    {
        if (isCensored || isCuttingMode) return;

        gameManager.EnterCuttingMode(this);
        if (isCut) 
        {
            gameManager.UpdateCensorTargets(originalText);
            isCut = false;
        }
        else
        {
            CuttingModeEffect(true);
        }
        
    }

    public void CuttingModeEffect(bool active)
    {
        if (active && !isCuttingMode)
        {
            isCuttingMode = true;
            Debug.Log("Cutting mode effect applied to: " + gameObject.name);
        }
        else if (!active && isCuttingMode)
        {
            isCuttingMode = false;
            Debug.Log("Cutting mode effect removed from: " + gameObject.name);
        }
    }
}