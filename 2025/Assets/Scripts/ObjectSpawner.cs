using System;
using Unity.Collections;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject mediaObject;     
    [SerializeField] private GameObject mediaSpawner;     

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        Instantiate(mediaObject, mediaSpawner.transform.position, Quaternion.identity);
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
