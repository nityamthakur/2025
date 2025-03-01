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

    public float zoomFactor = 1.25f;
    public float zoomSpeed = 0.2f;
    public bool canZoom = true;
    public bool stopZoom = true;
    public Entity entityComponent = null;
    public GameObject backOfNewspaper;

    private Collider2D newspaperCollider; // Reference to the collider
    private Draggable draggableScript; // Reference to the Draggable script
    private GameManager gameManager;

    void Start()
    {
        mainCamera = Camera.main;
        gameManager = FindFirstObjectByType<GameManager>();
        originalScale = transform.localScale;
        originalPosition = transform.position;
        newspaperCollider = GetComponent<Collider2D>();
        draggableScript = GetComponent<Draggable>(); // Get the Draggable script

        zoomPosition = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y + 2.25f, originalPosition.z);

        entityComponent = GetComponent<Entity>(); // Get the Entity script
        if (entityComponent == null)
        {
            Debug.LogError("Entity component not found on Newspaper!");
        }

        if (backOfNewspaper == null)
        {
            Debug.LogError("Back of Newspaper not assigned!");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && canZoom && stopZoom) // Right-click to zoom
        {
            canZoom = false;
            ToggleZoom();
        }
    }

    void ToggleZoom()
    {
        zoomFactor = 1.25f;
        zoomScale = originalScale * zoomFactor;
        //zoomScale = new Vector3(originalScale.x * zoomFactor, originalScale.y * (zoomFactor / 1.75f), originalScale.z * zoomFactor);
        // Zoom Out
        if (isZoomedIn)
        {
            if (entityComponent)
                entityComponent.ChangeMediaRotation(60);

            StartCoroutine(SmoothTransition(previousPosition, originalScale));

            if (draggableScript != null)
                draggableScript.enabled = true;  // Re-enable dragging

            backOfNewspaper.SetActive(false);
            gameManager.SetCensorFunctionality(false);
            entityComponent.SetBlur(true);
        }
        // Zoom In
        else
        {
            if (entityComponent)
                entityComponent.ChangeMediaRotation(-60);

            StartCoroutine(SmoothTransition(zoomPosition, zoomScale));
            previousPosition = transform.position;

            if (draggableScript != null)
                draggableScript.enabled = false; // Disable dragging

            backOfNewspaper.SetActive(true);
            gameManager.SetCensorFunctionality(true);
            entityComponent.SetBlur(false);
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

        if (!isZoomedIn)
            newspaperCollider.enabled = true; // Enable collision after transition occurs if not zoomed in
    }

    // To stop zoom fully, independent of canZoom
    public void preventZoom()
    {
        stopZoom = false;
    }
}
