using UnityEngine;

public class NewspaperZoom : MonoBehaviour
{
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Vector3 previousPosition;
    private bool isZoomedIn = false;
    private Camera mainCamera;
    private Vector3 zoomPosition;
    private Vector3 zoomScale;

    public float zoomFactor = 2.0f;
    public float zoomSpeed = 0.2f;
    public bool canZoom = true;
    public bool stopZoom = true;
    public Entity entityComponent = null;

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

        entityComponent = GetComponent<Entity>(); // Get the Entity script
        if (entityComponent == null)
        {
            Debug.LogError("Entity component not found on Newspaper!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canZoom && stopZoom)
        {
            canZoom = false;
            ToggleZoom();
        }
    }

    void ToggleZoom()
    {
        // Zoom Out
        if (isZoomedIn)
        {
            if(entityComponent) 
                entityComponent.ChangeMediaRotation(60);
            
            //StartCoroutine(SmoothTransition(originalPosition, originalScale));
            StartCoroutine(SmoothTransition(previousPosition, originalScale));
            //newspaperCollider.enabled = true;  // Re-enable collision

            if (draggableScript != null) 
                draggableScript.enabled = true;  // Re-enable dragging
        }
        // Zoom In
        else
        {
            if(entityComponent) 
                entityComponent.ChangeMediaRotation(-60);

            StartCoroutine(SmoothTransition(zoomPosition, zoomScale));
            previousPosition = transform.position;
            //newspaperCollider.enabled = false; // Disable collision

            if (draggableScript != null) 
                draggableScript.enabled = false; // Disable dragging
        }

    }

    System.Collections.IEnumerator SmoothTransition(Vector3 targetPos, Vector3 targetScale)
    {
        newspaperCollider.enabled = false; // Disable collision before transition occurs

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
    
        canZoom = true;

        isZoomedIn = !isZoomedIn;

        if(!isZoomedIn)
            newspaperCollider.enabled = true; // Enable collision after transition occurs if not zoomed in
    }

    // To stop zoom fully, independent of canZoom
    public void preventZoom()
    {
        stopZoom = false;
    }
}
