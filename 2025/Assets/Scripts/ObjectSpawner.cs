using System;
using Unity.Collections;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject mediaObject;     
    [SerializeField] private GameObject mediaSpawner;
    [SerializeField] private GameObject currentMediaObject; 
    [SerializeField] private GameObject splinePath;

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
        if (jobScene.jobDetails.numMediaProcessed++ >= jobScene.jobDetails.numMediaNeeded) {
            jobScene.ShowResults();
            return;
        }

        if (quitting) return;

        Debug.Log($"Spawning object at: {mediaSpawner.transform.position}");

        // Create new media object
        GameObject newMedia = Instantiate(mediaObject, mediaSpawner.transform.position, Quaternion.identity, currentMediaObject.transform);

        // Pass the spline prefab reference
        Entity mediaEntity = newMedia.GetComponent<Entity>();
        if (mediaEntity != null) {
            mediaEntity.SetSplinePrefab(splinePath); // Pass the splinePath prefab reference
        } else {
            Debug.LogError("Spawned media object is missing the Entity script!");
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
