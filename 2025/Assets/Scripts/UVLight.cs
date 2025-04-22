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
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on the UVLight object.");
            return;
        }
        defColor = spriteRenderer.color;

        targetCollider = gameManager.GetUVLightTarget();
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

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other == targetCollider)
        {
            Bounds uvLightBounds = GetComponent<Collider2D>().bounds;
            Bounds targetBounds = targetCollider.bounds;

            // Check if the UVLight bounds are completely contained within the target bounds
            isOverlapping = uvLightBounds.Contains(targetBounds.min) && uvLightBounds.Contains(targetBounds.max);

            if (isOverlapping)
            {
                Debug.Log("UVLight is overlapping with the target collider.");
                // Add any additional logic here, e.g., triggering an event or effect
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == targetCollider)
        {
            Debug.Log("UVLight exited the target collider.");
            
            isOverlapping = false;
        }
    }

    private void CheckClickOnTarget(Vector3 mousePosition)
    {
        // Perform a raycast at the mouse position to check if the targetCollider was clicked
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("HiddenImage"));

        if (hit.collider != null && hit.collider == targetCollider)
        {
            Debug.Log("Player clicked on the overlapping target object!");
            // Add any additional logic here, e.g., triggering an event or effect
            
            StartCoroutine(ChangeLightColor());

            hit.collider.gameObject.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.None;
            hit.collider.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    IEnumerator ChangeLightColor() 
    {
        spriteRenderer.color = new Color(Color.green.r, Color.green.g, Color.green.b, defColor.a);
        yield return new WaitForSeconds(blinkDuration);
        spriteRenderer.color = defColor;
    }
}
