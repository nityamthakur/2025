using System;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject mediaObject;     
    [SerializeField] private GameObject mediaSpawner;
    [SerializeField] private GameObject splinePath;

    // Quick fix for preventing object spawn on game close
    //private int currDay;
    private int newspaperPos = 0;
    private Entity.Newspaper[] newspapers;
    private Entity.Newspaper currentNewspaper;
    //public JobScene jobScene; 
    private GameManager gameManager;

    bool quitting = false;

    void Awake()
    {
        if (mediaObject == null || mediaSpawner == null || splinePath == null)
        {
            Debug.LogError("Media Object, Media Spawner, and/or Spline Path is not assigned.");
        }
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void StartMediaSpawn()
    {
        if(gameManager.GetJobDetails() == null) {
            Debug.LogError("jobDetails is null.");
        }

        LoadJsonFromFile();
        PassBanCensorLists();
        SpawnNewMediaObject();
    }

    private void SpawnNewMediaObject() {
        if (gameManager.IsDayEnded()) return;

        if (quitting) return;

        //Debug.Log($"Spawning object at: {mediaSpawner.transform.position}");

        // Create new media object
        GameObject newMedia = Instantiate(mediaObject, mediaSpawner.transform.position, Quaternion.identity);

        ReadNextNewspaper();

        // Pass the spline prefab reference
        Entity mediaEntity = newMedia.GetComponent<Entity>();
        if (mediaEntity != null) {
            StartCoroutine(DelayedPassNewspaperData(mediaEntity, currentNewspaper));
            mediaEntity.SetSplinePrefab(splinePath); // Pass the splinePath prefab reference
        } else {
            Debug.LogError("Spawned media object is missing the Entity script!");
        }
    }

    private IEnumerator DelayedPassNewspaperData(Entity mediaEntity, Entity.Newspaper newspaper)
    {
        yield return null; // Wait for the next frame
        mediaEntity.PassNewspaperData(newspaper);
    }

    private void LoadJsonFromFile()
    {
        // Check if Json is found in StreamingAssets folder
        string path = Path.Combine(Application.streamingAssetsPath, "GameText.json");
        if (!File.Exists(path))
        {
            Debug.LogError("JSON file not found at: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        ParseJson(json);
    }

    private void ParseJson(string json)
    {
        var jsonObject = JsonUtility.FromJson<Wrapper>(json);

        if (jsonObject != null && jsonObject.newspaperText.Count > 0)
        {
            newspapers = GetNewspapersForDay(jsonObject.newspaperText, gameManager.GetCurrentDay());
            newspaperPos = 0;
        }
        else
        {
            Debug.LogError("JSON parsing failed.");
        }
    }

    private Entity.Newspaper[] GetNewspapersForDay(List<Entry> entries, int day)
    {
        foreach (var entry in entries)
        {
            if (entry.day == day)
            {
                return entry.newspapers;
            }
        }
        return new Entity.Newspaper[0];
    }

    private void ReadNextNewspaper()
    {
        if (newspaperPos < newspapers.Length)
        {
            currentNewspaper = newspapers[newspaperPos];
            newspaperPos++;
        }
        else
        {
            // Destroy UI object or hide.
            Debug.Log("End of newspapers.");
        }
    }

    private void PassBanCensorLists()
    {
        List<string> banWords = new List<string>();
        List<string> censorWords = new List<string>();
        
        foreach (var newspaper in newspapers)
        {
            foreach (string word in newspaper.banWords)
            {
                banWords.Add(word);
            }
            foreach (string word in newspaper.censorWords)
            {
                censorWords.Add(word);
            }
        }

        gameManager.SetBanTargetWords(banWords.ToArray());
        gameManager.SetCensorTargetWords(censorWords.ToArray());
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

    [Serializable]
    private class Wrapper
    {
        public List<Entry> newspaperText;
    }

    [Serializable]
    private class Entry
    {
        public int day;
        public Entity.Newspaper[] newspapers;
    }
}
