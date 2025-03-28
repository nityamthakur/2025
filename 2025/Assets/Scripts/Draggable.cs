using UnityEngine;

public class Draggable : MonoBehaviour
{
    private bool isDragging = false;
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

        // Calculate screen bounds and object half-size
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        playerHalfWidth = boxCollider.bounds.extents.x;
        playerHalfHeight = boxCollider.bounds.extents.y;
    }

    public bool IsDragging()
    {
        return isDragging;
    }

    void OnMouseDown()
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

    void OnMouseUp()
    {
        isDragging = false;

        // Re-enable physics by setting the body type back to Dynamic
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
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