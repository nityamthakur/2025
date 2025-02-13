using UnityEngine;
using UnityEngine.EventSystems;

public class CensorTarget : MonoBehaviour, IPointerClickHandler
{
    private SpriteRenderer spriteRenderer;
    private bool isCensored = false;
    private bool isCensorTarget = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetToCensorTarget()
    {
        isCensorTarget = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isCensored) 
        {
            spriteRenderer.enabled = false;
            isCensored = false;
            if (isCensorTarget)
            {
                GameManager.Instance.CensorTargetDisabled();
            }
            else {
                GameManager.Instance.NonCensorTargetDisabled();
            }
        }
        else
        {
            spriteRenderer.enabled = true;
            isCensored = true;
            if (isCensorTarget)
            {
                GameManager.Instance.CensorTargetEnabled();
            }
            else {
                GameManager.Instance.NonCensorTargetEnabled();
            }
        }
    }
}