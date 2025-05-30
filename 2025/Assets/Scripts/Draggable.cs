using System.Collections;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    private bool isDragging = false;
    public bool CanDrag { get; set; }
    public bool DraggingDelay { get; set; }
    private Vector3 offset;
    private Rigidbody2D rb;

    private Vector2 screenBounds;
    private float playerHalfWidth;
    private float playerHalfHeight;
    private BoxCollider2D boxCollider;

    void Start()
    {
        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D is missing on the draggable object!");
        }

        // Get BoxCollider2D for size reference
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogError("BoxCollider2D is missing on the draggable object!");
            return;
        }

        DraggingDelay = false;
        CanDrag = true;

        // Calculate screen bounds and object half-size
        GetScreenBounds();
    }

    private void GetScreenBounds()
    {
        GameObject boundsImage = GameObject.FindWithTag("GameScreenSize");
        if (boundsImage == null)
        {
            Debug.Log("GameScreenSize tag not found.");
            return;
        }

        RectTransform rectTransform = boundsImage.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.Log("Object does not have RectTransform.");
            return;
        }

        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);

        // Bottom-left to top-right corner
        Vector3 bottomLeft = worldCorners[0];
        Vector3 topRight = worldCorners[2];

        screenBounds = (topRight - bottomLeft) / 2f;

        playerHalfWidth = boxCollider.bounds.extents.x;
        playerHalfHeight = boxCollider.bounds.extents.y;
    }


    public bool IsDragging()
    {
        return isDragging;
    }

    void OnMouseDown()
    {
        if (CanDrag)
        {
            isDragging = true;
            offset = transform.position - GetMouseWorldPosition();

            // Disable physics while dragging by setting the body type to Kinematic
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
        StartCoroutine(ReleaseBuffer());

        // Re-enable physics by setting the body type back to Dynamic
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private IEnumerator ReleaseBuffer()
    {
        DraggingDelay = true;
        yield return new WaitForSeconds(0.1f);
        DraggingDelay = false;
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 newPosition = GetMouseWorldPosition() + offset;

            // 🔹 Clamp within screen bounds
            newPosition.x = Mathf.Clamp(newPosition.x, -screenBounds.x + playerHalfWidth, screenBounds.x - playerHalfWidth);
            newPosition.y = Mathf.Clamp(newPosition.y, -screenBounds.y + playerHalfHeight, screenBounds.y - playerHalfHeight);

            transform.position = newPosition;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}