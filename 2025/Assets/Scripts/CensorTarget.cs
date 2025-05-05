using UnityEngine;
using UnityEngine.EventSystems;

public class CensorTarget : MonoBehaviour, IPointerClickHandler
{
    private SpriteRenderer spriteRenderer;
    private bool isCensored = false;
    private bool isCensorTarget = false;

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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!selectedToolManager.CanCensor() || selectedToolManager.GetSelectedTool() != "CensorPen") return;
        
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
}