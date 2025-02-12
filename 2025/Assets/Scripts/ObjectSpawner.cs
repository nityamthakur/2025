using System;
using Unity.Collections;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject mediaObject;     
    [SerializeField] private GameObject mediaSpawner;
    [SerializeField] private GameObject currentMediaObject; 
    private float downwardForce = 0.0005f;
    private float rightwardForce = 0.002f;
    // Quick fix for preventing object spawn on game close
    private int currDay;
    public JobScene jobScene; 

    bool quitting = false;

    void Awake()
    {
        if (currentMediaObject == null) 
        {
            // Throw error
            Debug.LogError("Current Media Object is not assigned.");
        }
    }

    public void StartMediaSpawn(int day, JobScene workScene)
    {
        currDay = day;
        jobScene = workScene;

        if(jobScene == null) {
            Debug.LogError("jobScene is null.");
        }

        if(jobScene.jobDetails == null) {
            Debug.LogError("jobScene.jobDetails is null.");
        }

        SpawnNewMediaObject();
    }

    private void SpawnNewMediaObject() {
        // Prevent spawning when player has reached the media process goal
        if (jobScene.jobDetails.numMediaProcessed++ >= jobScene.jobDetails.numMediaNeeded) {
            jobScene.ShowResults();
            return;
        }

        // Prevent spawning errors when exiting Unity play mode
        if(quitting) {
            return;
        }
        
        Debug.Log($"Spawning object at: {mediaSpawner.transform.position}");

        // Create a new GameObject and assign the sprite
        GameObject newMedia = Instantiate(mediaObject, mediaSpawner.transform.position, Quaternion.identity, currentMediaObject.transform);        

        // Get the Rigidbody component on the instantiated object
        Rigidbody2D rb = newMedia.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Apply downward force
            rb.AddForce(Vector3.down * downwardForce, ForceMode2D.Impulse);
            rb.AddForce(Vector3.right * rightwardForce, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogError("No Rigidbody found on the instantiated object!");
        }

    }

    void OnApplicationQuit ()
    {
        quitting = true;
    }

    // EventManager for creating a new media object after one gets destroyed
    private void OnEnable()
    {
        EventManager.OnMediaDestroyed += HandleMediaDestroyed;
    }

    private void OnDisable()
    {
        EventManager.OnMediaDestroyed -= HandleMediaDestroyed;
    }

    private void HandleMediaDestroyed(GameObject customer)
    {
        SpawnNewMediaObject();
    }
}
