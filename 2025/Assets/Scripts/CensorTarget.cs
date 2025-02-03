using UnityEngine;
using UnityEngine.EventSystems;

public class CensorTarget : MonoBehaviour, IPointerClickHandler
{
    private SpriteRenderer spriteRenderer;
    

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        spriteRenderer.enabled = true;

        GameManager.Instance.CensorTargetClicked();
    }
}