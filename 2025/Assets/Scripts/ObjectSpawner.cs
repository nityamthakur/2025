using System;
using Unity.Collections;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject mediaObject;     
    [SerializeField] private GameObject mediaSpawner;
    [SerializeField] private GameObject currentMediaObject; 
    private float downwardForce = 0.001f;
    // Quick fix for preventing object spawn on game close
    bool quitting = false;

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
        //newMedia.transform.Rotate(90.0f, 0.0f, 0.0f, Space.Self);
        
        // Get the Entity component to rotate the object
        Entity entityComponent = newMedia.GetComponent<Entity>();
        if (entityComponent != null)
        {
            // Rotation of media object being wonky with the censor bars
            //entityComponent.ChangeMediaRotation(60);
        }
        else
        {
            Debug.LogError("Entity component not found on newMedia!");
        }

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
        if(quitting) return;
        EventManager.OnMediaDestroyed -= HandleMediaDestroyed;
    }

    private void HandleMediaDestroyed(GameObject customer)
    {
        if (quitting) return; // Prevent spawning when quitting
        SpawnNewMediaObject();
    }
}
