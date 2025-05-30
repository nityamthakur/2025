using UnityEngine;
using System.Collections;

public class MediaZoom : MonoBehaviour
{
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Vector3 previousPosition;
    private bool isZoomedIn = false;
    public bool IsZoomedIn
    {
        get { return isZoomedIn; }
        private set { isZoomedIn = value; }
    }
    private Camera mainCamera;
    private Vector3 zoomPosition;
    private Vector3 zoomScale;

    public float zoomFactor = 10.0f;
    public float zoomSpeed = 0.2f;
    public bool canZoom = true;
    public bool AllowZoom { get; set; }
    private bool currentlyZooming = false;

    [SerializeField] private ImageObject entityComponent;
    [SerializeField] private Collider2D newspaperCollider;
    [SerializeField] private Draggable draggableScript;
    private GameManager gameManager;

    void Start()
    {
        mainCamera = Camera.main;
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager is not found in the scene.");
            return;
        }

        originalScale = transform.localScale;
        originalPosition = transform.position;

        zoomPosition = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y / 2, originalPosition.z);
    }

    public void StartZoom()
    {
        if (canZoom && AllowZoom && Time.timeScale != 0)
        {
            canZoom = false;
            ToggleZoom();
        }
    }

    private void OnEnable()
    {
        EventManager.UnZoomObject += UnZoomObject;
    }

    private void OnDisable()
    {
        EventManager.UnZoomObject -= UnZoomObject;
    }

    private void UnZoomObject()
    {
        if (isZoomedIn && !currentlyZooming)
        {
            currentlyZooming = true;
            zoomScale = originalScale * zoomFactor;

            entityComponent.ObjectGravityOn(true);

            StartCoroutine(SmoothTransition(previousPosition, originalScale));

            if (draggableScript != null)
                draggableScript.enabled = true;

        }
    }

    void ToggleZoom()
    {
        currentlyZooming = true;
        EventManager.UnZoomObject?.Invoke();
        EventManager.PlaySound?.Invoke("newspaperRustling", true);
        zoomScale = originalScale * 2.5f;

        if (isZoomedIn)
        {
            entityComponent.ObjectGravityOn(true);

            StartCoroutine(SmoothTransition(previousPosition, originalScale));

            if (draggableScript != null)
                draggableScript.enabled = true;

        }
        else
        {
            entityComponent.ObjectGravityOn(false);

            StartCoroutine(SmoothTransition(zoomPosition, zoomScale));
            previousPosition = transform.position;

            if (draggableScript != null)
                draggableScript.enabled = false;

        }
    }

    IEnumerator SmoothTransition(Vector3 targetPos, Vector3 targetScale)
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

        canZoom = true;
        isZoomedIn = !isZoomedIn;

        if (!isZoomedIn)
            newspaperCollider.enabled = true;
        else
            transform.position = zoomPosition;

        currentlyZooming = false;
    }

    private void OnDestroy()
    {

    }
}