using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class NewspaperZoom : MonoBehaviour
{
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private bool isZoomedIn = false;
    private Camera mainCamera;
    private Vector3 zoomPosition;
    private Vector3 zoomScale;

    public float zoomFactor = 2.0f;
    public float zoomSpeed = 0.2f;

    private Collider2D newspaperCollider; // Reference to the collider
    private Draggable draggableScript; // Reference to the Draggable script

    void Start()
    {
        mainCamera = Camera.main;
        originalScale = transform.localScale;
        originalPosition = transform.position;
        newspaperCollider = GetComponent<Collider2D>();
        draggableScript = GetComponent<Draggable>(); // Get the Draggable script

        zoomScale = originalScale * zoomFactor;
        zoomPosition = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, originalPosition.z);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleZoom();
        }
    }

    void ToggleZoom()
    {
        Entity entityComponent = GetComponent<Entity>(); // Get the Entity script

        if (isZoomedIn)
        {
            if (entityComponent != null)
            {
                entityComponent.ChangeMediaRotation(60); // Reset rotation when zooming out
            }
            else
            {
                Debug.LogError("Entity component not found on Newspaper!");
            }

            StartCoroutine(SmoothTransition(originalPosition, originalScale));
            newspaperCollider.enabled = true;  // Re-enable collision
            if (draggableScript != null) draggableScript.enabled = true;  // Re-enable dragging
        }
        else
        {
            if (entityComponent != null)
            {
                entityComponent.ChangeMediaRotation(-60); // Reset rotation when zooming out
            }
            else
            {
                Debug.LogError("Entity component not found on Newspaper!");
            }

            StartCoroutine(SmoothTransition(zoomPosition, zoomScale));
            newspaperCollider.enabled = false; // Disable collision
            if (draggableScript != null) draggableScript.enabled = false; // Disable dragging
        }

        isZoomedIn = !isZoomedIn;
    }

    System.Collections.IEnumerator SmoothTransition(Vector3 targetPos, Vector3 targetScale)
    {
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / zoomSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.position = targetPos;
        transform.localScale = targetScale;
    }
}
