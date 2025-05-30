using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ImageObject : MonoBehaviour
{
    private MediaSplinePath currSplinePath;
    private GameObject splinePrefab;
    private Draggable draggableScript;
    private Rigidbody2D rigidBody;
    private BoxCollider2D boxCollider;
    [SerializeField] private MediaZoom zoomComponent;
    [SerializeField] private Canvas canvas;
    public bool takeActionOnDestroy = false;
    public bool beingDestroyed = false;
    private Vector2 screenBounds;
    private float playerHalfWidth;
    private float playerHalfHeight;
    private bool isInteractable = true;

    private void Awake()
    {
        draggableScript = GetComponent<Draggable>(); // Get the Draggable script
        Canvas canvas = GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
        }
    }

    private void Start()
    {
        ChangeMediaRotation(60);

        TryGetComponent<Rigidbody2D>(out var Urigid);
        rigidBody = Urigid;

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

        // Calculate half-width and half-height in world space
        screenBounds = (topRight - bottomLeft) / 2f;

        // Setting object boundaries to keep it inside the screen
        TryGetComponent<BoxCollider2D>(out var Ucollider);
        boxCollider = Ucollider;

        playerHalfWidth = boxCollider.bounds.extents.x;
        playerHalfHeight = boxCollider.bounds.extents.y;

    }

    /*public void ChangeMediaRotation( int angleX )
    {
        Debug.Log("Angle");
        transform.eulerAngles = new Vector3(
        transform.eulerAngles.x + angleX,
        transform.eulerAngles.y,
        transform.eulerAngles.z);
    }*/

    public void ChangeMediaRotation(int targetAngleX, float duration = 0.2f)
    {
        StartCoroutine(RotateOverTime(targetAngleX, duration));
    }

    public void ObjectGravityOn(bool on)
    {
        if (rigidBody == null) return;

        if (on)
        {
            rigidBody.gravityScale = 3f; // Enable gravity
        }
        else
        {
            rigidBody.gravityScale = 0f; // Disable gravity
            rigidBody.linearVelocity = Vector2.zero; // Stop all movement
            rigidBody.angularVelocity = 0f; // Stop any rotation momentum
        }
    }

    private IEnumerator RotateOverTime(int targetAngleX, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startRotation = transform.eulerAngles;
        Vector3 targetRotation = new Vector3(
            startRotation.x + targetAngleX,
            startRotation.y,
            startRotation.z);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            transform.eulerAngles = Vector3.Lerp(startRotation, targetRotation, t);
            yield return null;
        }

        transform.eulerAngles = targetRotation;
    }


    public void SetUpSplinePath(GameObject prefab)
    {
        draggableScript.enabled = false;

        // Create a new spline path
        splinePrefab = prefab;
        if (splinePrefab == null)
        {
            Debug.LogError("Spline prefab not assigned correctly in Entity.");
        }

        GameObject newSplinePath = Instantiate(splinePrefab);
        currSplinePath = newSplinePath.GetComponent<MediaSplinePath>();

        if (currSplinePath != null)
        {
            currSplinePath.EntranceMovement(transform);
            StartCoroutine(SpawnDelay(currSplinePath.GetDuration()));
        }
        else
        {
            Debug.LogError("MediaSplinePath component is missing on instantiated spline.");
        }
    }

    IEnumerator SpawnDelay(float duration)
    {
        draggableScript.enabled = false;
        zoomComponent.AllowZoom = false;
        yield return new WaitForSeconds(duration);
        zoomComponent.AllowZoom = true;
        ChangeMediaRotation(-60);
        ObjectGravityOn(true);
        draggableScript.enabled = true;
    }

    private void OnMouseOver()
    {
        if (isInteractable)
        {
            if (Input.GetMouseButtonDown(1))
            {
                MoveToFront();
                zoomComponent.StartZoom();
            }
            if (Input.GetMouseButtonDown(0))
            {
                MoveToFront();
            }
        }
    }

    private void MoveToFront()
    {
        EventManager.ResetLayerOrder?.Invoke();
        canvas.sortingOrder = 5;
    }

    private void ResetLayerOrder()
    {
        canvas.sortingOrder = 1;
    }

    private void CanInteractWithObject(bool able)
    {
        isInteractable = able;
        draggableScript.CanDrag = able;
    }

    private void OnEnable()
    {
        EventManager.ResetLayerOrder += ResetLayerOrder;
        EventManager.CanInteractWithObject += CanInteractWithObject;
    }

    private void OnDisable()
    {
        EventManager.ResetLayerOrder -= ResetLayerOrder;
        EventManager.CanInteractWithObject -= CanInteractWithObject;
    }

    private bool isInsideTrigger = false;
    private Collider2D storedTrigger = null;

    /*
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("DropBoxAccept") || other.gameObject.CompareTag("DropBoxDestroy"))
        {
            isInsideTrigger = true;
            storedTrigger = other;
            if (other.gameObject.CompareTag("DropBoxAccept"))
                EventManager.GlowingBoxShow?.Invoke("accept", true);
            if (other.gameObject.CompareTag("DropBoxDestroy"))
                EventManager.GlowingBoxShow?.Invoke("destroy", true);
        }
    }
    */
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isInsideTrigger && other.CompareTag("DropBoxAccept") && draggableScript.IsDragging())
        {
            EventManager.GlowingBoxShow?.Invoke("accept", true);
            isInsideTrigger = true;
            storedTrigger = other;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == storedTrigger)
        {
            isInsideTrigger = false;
            storedTrigger = null;
            if (other.gameObject.CompareTag("DropBoxAccept"))
                EventManager.GlowingBoxShow?.Invoke("accept", false);
        }

    }

    private void Update()
    {
        if (isInsideTrigger && storedTrigger != null && !draggableScript.IsDragging() && draggableScript.DraggingDelay) // Check if dragging has stopped inside trigger
        {
            if (storedTrigger.gameObject.CompareTag("DropBoxAccept"))
            {
                zoomComponent.AllowZoom = false;
                StartCoroutine(DestroyAfterExitMovement("Accept"));
            }

            // Prevent multiple triggers
            isInsideTrigger = false;
        }

        if (!beingDestroyed)
        {
            Transform imageComponent = transform.Find("ImageComponent");
            if (imageComponent != null)
            {
                Vector3 pos = imageComponent.position;
                pos.x = Mathf.Clamp(pos.x, -screenBounds.x + playerHalfWidth, screenBounds.x - playerHalfWidth);
                pos.y = Mathf.Clamp(pos.y, -screenBounds.y + playerHalfHeight, screenBounds.y - playerHalfHeight);
                imageComponent.position = pos;
            }
            else
            {
                Vector3 pos = transform.position;
                pos.x = Mathf.Clamp(pos.x, -screenBounds.x + playerHalfWidth, screenBounds.x - playerHalfWidth);
                pos.y = Mathf.Clamp(pos.y, -screenBounds.y + playerHalfHeight, screenBounds.y - playerHalfHeight);
                transform.position = pos;
            }
        }
    }

    private IEnumerator DestroyAfterExitMovement(string box)
    {
        beingDestroyed = true;
        ChangeMediaRotation(60);
        // Turn of Rigidbody because newspaper gets wierd when colliding with boxes
        if (TryGetComponent<Rigidbody2D>(out var rigidBody))
        {
            rigidBody.simulated = false; // Disables physics interactions without removing Rigidbody2D
        }

        draggableScript.enabled = false;
        if (box == "Accept")
        {
            currSplinePath.ExitMovementAccept(transform);
        }
        else
        {
            currSplinePath.ExitMovementDestroy(transform);
        }

        yield return new WaitForSeconds(0.5f);
        EventManager.PlaySound?.Invoke("tossPaper", true);
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Ensure the spline path is destroyed when media is destroyed
        if (currSplinePath != null)
        {
            Destroy(currSplinePath.gameObject);
        }

        if (takeActionOnDestroy)
        {
            EventManager.OnImageDestroyed?.Invoke();
        }
    }
}