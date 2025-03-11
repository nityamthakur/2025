using System.Collections;
using UnityEngine;

public class ImageObject : MonoBehaviour
{
    private MediaSplinePath currSplinePath;
    private GameObject splinePrefab;
    private Draggable draggableScript;
    public bool takeActionOnDestroy = false;
    
    private void Start()
    {
        ChangeMediaRotation( 60 );  
    }

    public void ChangeMediaRotation( int angleX )
    {
        transform.eulerAngles = new Vector3(
        transform.eulerAngles.x + angleX,
        transform.eulerAngles.y,
        transform.eulerAngles.z);
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
            currSplinePath.EntranceMovement(transform);
        }
        else
        {
            Debug.LogError("MediaSplinePath component is missing on instantiated spline.");
        }
        draggableScript.enabled = true; 
        ChangeMediaRotation( -60 );  

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("DropBoxAccept"))
        {
            ChangeMediaRotation( 60 );  
            StartCoroutine(DestroyAfterExitMovement("Accept"));
        }
        else if (collision.gameObject.CompareTag("DropBoxDestroy"))
        {
            ChangeMediaRotation( 60 );  
            StartCoroutine(DestroyAfterExitMovement("Destroy"));
        }
    }

    private IEnumerator DestroyAfterExitMovement(string box)
    {
        EventManager.PlaySound?.Invoke("tossPaper");
        // Turn of Rigidbody because newspaper gets wierd when colliding with boxes
        if (TryGetComponent<Rigidbody2D>(out var rigidBody))
        {
            rigidBody.simulated = false; // Disables physics interactions without removing Rigidbody2D
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                0,
                transform.eulerAngles.z);
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

        // Wait for the movement to complete (adjust time if needed)
        yield return new WaitForSeconds(1.5f);

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