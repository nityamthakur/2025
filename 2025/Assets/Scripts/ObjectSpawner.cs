using System;
using Unity.Collections;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject mediaObject;     
    [SerializeField] private GameObject mediaSpawner;
    [SerializeField] private GameObject currentMediaObject; 
    private float downwardForce = 0.001f;

    void Start()
    {
        if (currentMediaObject == null) 
        {
            // Throw error
            Debug.LogError("Current Media Object is not assigned.");
        }

        SpawnNewMediaObject();
    }

    void SpawnNewMediaObject() {
        if (mediaSpawner == null)
        {
            Debug.LogError("MediaSpawner GameObject is not assigned.");
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
        }
        else
        {
            Debug.LogError("No Rigidbody found on the instantiated object!");
        }

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
