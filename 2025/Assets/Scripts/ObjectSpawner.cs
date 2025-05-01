using System;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using TMPro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject mediaObject;
    [SerializeField] private GameObject mediaSpawner;
    [SerializeField] private GameObject splinePath;
    [SerializeField] private GameObject rentNoticePrefab;
    [SerializeField] private GameObject imageObject;

    // Quick fix for preventing object spawn on game close
    //private int currDay;
    private int newspaperPos = 0;
    private Entity.Newspaper[] newspapers;
    private Entity.Newspaper currentNewspaper;
    //public JobScene jobScene; 
    private GameManager gameManager;

    bool quitting = false;

    public void Initialize()
    {
        if (mediaObject == null || mediaSpawner == null || splinePath == null || rentNoticePrefab == null)
        {
            Debug.LogError("Media Object, Media Spawner, Spline Path, or Rent Notice is not assigned.");
        }
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void StartMediaSpawn()
    {
        if (gameManager.GetJobDetails() == null)
        {
            Debug.LogError("jobDetails is null.");
        }

        LoadJsonFromFile();
        PassBanCensorLists();
        SpawnNewMediaObject();
    }

    private void SpawnNewMediaObject()
    {
        if (gameManager.IsDayEnded())
            return;

        if (quitting)
            return;

        Debug.Log($"Spawning object at: {mediaSpawner.transform.position}");

        // Sound Effect: Paper Comes In
        EventManager.PlaySound?.Invoke("papercomein");

        // Create new media object
        GameObject newMedia = Instantiate(mediaObject, mediaSpawner.transform.position, Quaternion.identity);

        ReadNextNewspaper();

        // Pass the spline prefab reference
        Entity mediaEntity = newMedia.GetComponent<Entity>();
        if (mediaEntity != null)
        {
            StartCoroutine(DelayedPassNewspaperData(mediaEntity, currentNewspaper));
            mediaEntity.SetSplinePrefab(splinePath); // Pass the splinePath prefab reference
        }
        else
        {
            Debug.LogError("Spawned media object is missing the Entity script!");
        }

        if(quitting)
        {
            Destroy(newMedia);
        }

    }

    // For non media censoring objects like fliers and pamphlets
    public void SpawnImageObject(bool takeActionOnDestroy) 
    {

        // Create new media object
        GameObject newMedia = Instantiate(imageObject, mediaSpawner.transform.position, Quaternion.identity);
        
        // Pass the spline prefab reference
        ImageObject mediaEntity = newMedia.GetComponent<ImageObject>();
        if (mediaEntity != null) 
        {
            mediaEntity.takeActionOnDestroy = takeActionOnDestroy;
            mediaEntity.SetUpSplinePath(splinePath); // Pass the splinePath prefab reference
        } 
        else 
        {
            Debug.LogError("Spawned media object is missing the Entity script!");
        }
    }

    public void SpawnRentNotice()
    {
        // Instantiate RentNoticePrefab
        GameObject rentNoticeInstance = Instantiate(rentNoticePrefab);

        int rent = gameManager.gameData.rent;
        string rentText = $"Rent will cost {rent}. If you are unable to pay by the end of the day, expect to be evicted.";

        // Find BodyText directly under RentNoticePrefab
        TextMeshPro bodyText = rentNoticeInstance.transform.Find("BodyText")?.GetComponent<TextMeshPro>();
        if (bodyText != null)
        {
            bodyText.text = rentText;
        }
        else
        {
            Debug.LogError("SpawnRentNotice: Couldn't find BodyText!");
        }

        // Attach movement logic to RentNoticePrefab itself (since physics is on the parent now)
        ImageObject mediaEntity = rentNoticeInstance.GetComponent<ImageObject>();
        if (mediaEntity != null)
        {
            mediaEntity.SetUpSplinePath(splinePath); // Apply movement
        }
        else
        {
            Debug.LogError("SpawnRentNotice: RentNoticePrefab is missing Entity script!");
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
        //Our JSON deserialization needs to be significantly more complex to handle this.
        //Once all of the JSON is migrated over to the new format, it can be simplified
        try
        {
            //Load everything
            var jsonRoot = JObject.Parse(json);
            //Get just the newspaper text
            var newspaperTextArr = (JArray)jsonRoot["newspaperText"];

            if (newspaperTextArr == null)
            {
                Debug.LogError("Unable to find newspaper data in JSON file. Stop breaking my stuff!.");
                return;
            }

            //Get the current day obj
            var currentPapers = newspaperTextArr.Where(item => (int)item["day"] == gameManager.gameData.GetCurrentDay())
                .Cast<JObject>().FirstOrDefault();

            if (currentPapers == null)
            {
                Debug.LogError("Unable to find paper entries for day " + gameManager.gameData.GetCurrentDay());
                return;
            }

            //Now go through and get the paper data
            var newsPaperArr = (JArray)currentPapers["newspapers"];
            newspapers = new Entity.Newspaper[newsPaperArr.Count];

            for (var i = 0; i < newsPaperArr.Count; i++)
            {
                var newspaperObj = (JObject)newsPaperArr[i];
                var newspaper = new Entity.Newspaper
                {
                    date = newspaperObj["date"]?.ToString(),
                    hasHiddenImage = newspaperObj["hasHiddenImage"] != null && (bool)newspaperObj["hasHiddenImage"],
                    banWords = newspaperObj["banWords"]?.ToObject<string[]>() ?? Array.Empty<string>(),
                    censorWords = newspaperObj["censorWords"]?.ToObject<string[]>() ?? Array.Empty<string>()
                };

                //Add all of the possibly complex objects
                ProcessField(newspaperObj["publisher"], out newspaper.publisher, out newspaper.publisherIsComplex);
                ProcessField(newspaperObj["title"], out newspaper.title, out newspaper.titleIsComplex);
                ProcessField(newspaperObj["front"], out newspaper.frontContent, out newspaper.frontIsComplex);
                ProcessField(newspaperObj["back"], out newspaper.backContent, out newspaper.backIsComplex);

                newspapers[i] = newspaper;
            }

            newspaperPos = 0;
        }
        catch (Exception e)
        {
            Debug.LogError("Error while parsing newspaper JSON data: " + e.Message);
        }
    }

    private void ProcessField(JToken token, out string content, out bool isComplex)
    {
        if (token is JObject)
        {
            content = token.ToString();
            isComplex = true;
        }
        else
        {
            content = token?.ToString() ?? "";
            isComplex = false;
        }
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

        gameManager.SetBanTargetWords(banWords.Distinct().ToArray());
        gameManager.SetCensorTargetWords(censorWords.Distinct().ToArray());
    }

    void OnApplicationQuit()
    {
        quitting = true;
    }

    // EventManager for creating a new media object after one gets destroyed
    private void OnEnable()
    {
        EventManager.OnMediaDestroyed += HandleMediaDestroyed;
        GameManager.OnGameRestart += StopSpawningOnRestart; // Listen for restarts
    }

    private void OnDisable()
    {
        EventManager.OnMediaDestroyed -= HandleMediaDestroyed;
        GameManager.OnGameRestart -= StopSpawningOnRestart;
    }

    private void HandleMediaDestroyed(GameObject customer)
    {
        if (quitting)
            return;
        SpawnNewMediaObject();
    }
    
    private void StopSpawningOnRestart()
    {
        quitting = true;
    }
    
}
