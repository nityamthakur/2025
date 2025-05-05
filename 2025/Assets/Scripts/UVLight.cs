using UnityEngine;
using System.Collections;

public class UVLight : MonoBehaviour
{
    [SerializeField] private Collider2D targetCollider;
    [SerializeField] private float blinkDuration = 0.25f;
    private bool isOverlapping = false;
    private SpriteRenderer spriteRenderer;
    private Color defColor;
    private GameManager gameManager;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager is not found in the scene.");
            return;
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on the UVLight object.");
            return;
        }
        defColor = spriteRenderer.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0) return; 
        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos;

        // Check if the player clicks on hidden image
        if (isOverlapping && Input.GetMouseButtonDown(0))
        {
            CheckClickOnTarget(mousePos);
        }
    }

    public void SetTargetCollider(Collider2D newTargetCollider)
    {
        targetCollider = newTargetCollider;
    }

    private bool AreBoundsOverlapping(Bounds a, Bounds b)
    {
        return a.min.x <= b.max.x && a.max.x >= b.min.x &&
            a.min.y <= b.max.y && a.max.y >= b.min.y;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other == targetCollider)
        {
            Bounds uvLightBounds = GetComponent<Collider2D>().bounds;
            Bounds targetBounds = targetCollider.bounds;
            
            // Check if the UVLight bounds are contained within the target bounds
            isOverlapping = AreBoundsOverlapping(uvLightBounds, targetBounds);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == targetCollider)
        {
            isOverlapping = false;
        }
    }

    private void CheckClickOnTarget(Vector3 mousePosition)
    {
        // Perform a raycast at the mouse position to check if the targetCollider was clicked
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("HiddenImage"));

        if (hit.collider != null && hit.collider == targetCollider)
        {
            gameManager.SetTargetFound(true);
            
            StartCoroutine(ChangeLightColor());
            hit.collider.gameObject.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.None;
            hit.collider.gameObject.GetComponent<BoxCollider2D>().enabled = false;

            Debug.Log("Clicked on hidden image.");
        }
    }

    IEnumerator ChangeLightColor() 
    {
        spriteRenderer.color = new Color(Color.green.r, Color.green.g, Color.green.b, defColor.a);
        yield return new WaitForSeconds(blinkDuration);
        spriteRenderer.color = defColor;
    }
}
