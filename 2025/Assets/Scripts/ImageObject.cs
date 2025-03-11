using System.Collections;
using UnityEngine;

public class ImageObject : MonoBehaviour
{
    private MediaSplinePath currSplinePath;
    private GameObject splinePrefab;
    private Draggable draggableScript;
    private Rigidbody2D rigidBody;
    private BoxCollider2D boxCollider;
    public bool takeActionOnDestroy = false;
    public bool beingDestroyed = false;
    private Vector2 screenBounds;
    private float playerHalfWidth;
    private float playerHalfHeight;
    
    private void Start()
    {
        ChangeMediaRotation( 60 );
        TryGetComponent<Rigidbody2D>(out var Urigid);
        rigidBody = Urigid;

        // Setting object boundaries to keep it inside the screen
        TryGetComponent<BoxCollider2D>(out var Ucollider);
        boxCollider = Ucollider;
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
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
        draggableScript = GetComponent<Draggable>(); // Get the Draggable script
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
            currSplinePath.EntranceMovement(transform, () => 
            {
                draggableScript.enabled = true;
                ChangeMediaRotation(-60);
                rigidBody.gravityScale = 3f;  // Enable gravity (adjust as needed)
            });
        }
        else
        {
            Debug.LogError("MediaSplinePath component is missing on instantiated spline.");
        }
    }

    private bool isInsideTrigger = false;
    private Collider2D storedTrigger = null;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("DropBoxAccept") || other.gameObject.CompareTag("DropBoxDestroy"))
        {
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
        }
    }

    private void Update()
    {
        if (isInsideTrigger && storedTrigger != null && !draggableScript.IsDragging()) // Check if dragging has stopped inside trigger
        {
            if (storedTrigger.gameObject.CompareTag("DropBoxAccept"))
            {
                StartCoroutine(DestroyAfterExitMovement("Accept"));
            }
            else if (storedTrigger.gameObject.CompareTag("DropBoxDestroy"))
            {
                StartCoroutine(DestroyAfterExitMovement("Destroy"));
            }
        }

        if(!beingDestroyed)
        {
            Vector2 pos = transform.position;

            pos.x = Mathf.Clamp(pos.x, -screenBounds.x + playerHalfWidth, screenBounds.x - playerHalfWidth);
            pos.y = Mathf.Clamp(pos.y, -screenBounds.y + playerHalfHeight, screenBounds.y - playerHalfHeight);

            transform.position = pos;
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
        EventManager.PlaySound?.Invoke("tossPaper");
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
        
        if(takeActionOnDestroy)
        {
            EventManager.OnImageDestroyed?.Invoke();
        }
    }
}