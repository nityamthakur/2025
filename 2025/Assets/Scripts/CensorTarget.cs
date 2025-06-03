using UnityEngine;
using UnityEngine.EventSystems;
using System;
using TMPro;
using UnityEngine.UI;

public class CensorTarget : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Color censorColor = new Color(0, 0, 0, 1f);
    [SerializeField] private Color cutColor = new Color(255, 255, 0, 0.5f);
    private SpriteRenderer spriteRenderer;
    private bool isCensored = false;
    private bool isCensorTarget = false;
    private bool isReplaceTarget = false;
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
    public void SetToReplaceTarget()
    {
        isReplaceTarget = true;
    }
    public bool IsReplaceTarget()
    {
        return isReplaceTarget;
    }
    public void ToggleIsCut()
    {
        isCut = !isCut;
    }

    public TMP_Text GetTextComponent()
    {
        return textCmp;
    }
    public string GetOriginalText()
    {
        return originalText;
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
        if (isCut) return;
        
        if (isCensored) 
        {
            spriteRenderer.enabled = false;
            isCensored = false;
            gameManager.IncrementPenSlider();

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
            spriteRenderer.color = censorColor;
            spriteRenderer.enabled = true;
            isCensored = true;
            gameManager.DecrementPenSlider();

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
        if (isCensored) return;
        if (isCuttingMode) 
        {
            gameManager.ExitCuttingMode();
            return;
        }

        gameManager.EnterCuttingMode(this);
        if (isCut)
        {
            isCuttingMode = true;
            gameManager.UpdateCensorTargets(originalText);
            gameManager.IncrementKnifeSlider();

            if (isReplaceTarget)
            {
                gameManager.ReplaceTargetDisabled();
            }
            else
            {
                gameManager.NonReplaceTargetDisabled();
            }
        }
        else
        {
            CuttingModeEffect(true);

            // if (isReplaceTarget)
            // {
            //     gameManager.ReplaceTargetEnabled();
            // }
            // else 
            // {
            //     gameManager.NonReplaceTargetEnabled();
            // }
        }
        
    }

    public void CuttingModeEffect(bool active)
    {
        if (active && !isCuttingMode)
        {
            isCuttingMode = true;

            spriteRenderer.color = cutColor;
            spriteRenderer.enabled = true;
            Debug.Log("Cutting mode effect applied to: " + gameObject.name);
        }
        else if (!active && isCuttingMode)
        {
            isCuttingMode = false;

            spriteRenderer.enabled = isCut;
            Debug.Log("Cutting mode effect removed from: " + gameObject.name);
        }
    }
}