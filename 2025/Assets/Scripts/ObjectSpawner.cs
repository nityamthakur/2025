using System;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject mediaObject;
    [SerializeField] private GameObject mediaSpawner;
    [SerializeField] private GameObject splinePath;
    [SerializeField] private GameObject rentNoticePrefab;
    [SerializeField] private GameObject imageObject;
    public Sprite greenPartyLetter;
    private List<GameObject> rentNotices = new();
    private int newspaperPos = 0;
    private List<Entity.Newspaper> newspapers = new();
    private List<GameObject> spawnedMedia = new();
    private Entity.Newspaper currentNewspaper;
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
        //Reshuffle(newspapers);
        PassWordLists();
        SpawnNewMediaObject();
    }

    private void SpawnNewMediaObject()
    {
        if (gameManager.IsDayEnded())
            return;

        if (quitting)
            return;

        //Debug.Log($"Spawning object at: {mediaSpawner.transform.position}");

        // Sound Effect: Paper Comes In
        EventManager.PlaySound?.Invoke("papercomein", true);

        // Create new media object
        GameObject newMedia = Instantiate(mediaObject, mediaSpawner.transform.position, Quaternion.identity);

        ReadNextNewspaper();
        gameManager.gameData.AddReviewToGameData(currentNewspaper);

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

        if (quitting)
        {
            Destroy(newMedia);
        }

        gameManager.SetCurrentMediaObject(newMedia);
        spawnedMedia.Add(newMedia);
    }

    // For non media censoring objects like fliers and pamphlets
    public void SpawnImageObject(bool takeActionOnDestroy)
    {
        /*
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
        */
        // Create new media object
        GameObject rentNoticeInstance = Instantiate(rentNoticePrefab);
        Canvas prefabCanvas = rentNoticeInstance.GetComponentInChildren<Canvas>();
        if (prefabCanvas != null)
        {
            prefabCanvas.renderMode = RenderMode.WorldSpace;
            prefabCanvas.worldCamera = Camera.main;
        }
        rentNotices.Add(rentNoticeInstance);

        // Find BodyText directly under RentNoticePrefab
        Transform rentImage = rentNoticeInstance.transform.Find("ImageComponent");
        if (rentImage != null)
        {
            TextMeshProUGUI bodyText = rentImage.GetComponentInChildren<TextMeshProUGUI>();
            if (bodyText != null)
                bodyText.text = "";
            else
                Debug.LogError("SpawnRentNotice: Couldn't find BodyText!");

            // Change the image sprite
            Image imageComponent = rentImage.GetComponent<Image>();
            if (imageComponent != null)
                imageComponent.sprite = greenPartyLetter;
            else
                Debug.LogError("SpawnRentNotice: Couldn't find Image component!");

        }
        else
            Debug.LogError("SpawnRentNotice: Couldn't find ImageComponent!");

        // Attach movement logic to RentNoticePrefab itself (since physics is on the parent now)
        if (rentImage.TryGetComponent<ImageObject>(out var mediaEntity))
        {
            mediaEntity.SetUpSplinePath(splinePath); // Apply movement
            mediaEntity.takeActionOnDestroy = takeActionOnDestroy;
        }
        else
            Debug.LogError("SpawnRentNotice: RentNoticePrefab is missing Entity script!");

    }

    public void SpawnRentNotice()
    {
        // Instantiate RentNoticePrefab
        GameObject rentNoticeInstance = Instantiate(rentNoticePrefab);
        Canvas prefabCanvas = rentNoticeInstance.GetComponentInChildren<Canvas>();
        if (prefabCanvas != null)
        {
            prefabCanvas.renderMode = RenderMode.WorldSpace;
            prefabCanvas.worldCamera = Camera.main;
        }
        rentNotices.Add(rentNoticeInstance);


        int rent = gameManager.gameData.rent;
        string rentText = $"Dear Tenet,\n\nThis is a reminder that due to missed payments, your rent has been increased to <color=yellow>${rent}.</color>\n\nIf you are unable to pay by end of day, expect to be evicted.";

        // Find BodyText directly under RentNoticePrefab
        Transform rentImage = rentNoticeInstance.transform.Find("ImageComponent");
        if (rentImage != null)
        {
            TextMeshProUGUI bodyText = rentImage.GetComponentInChildren<TextMeshProUGUI>();
            if (bodyText != null)
                bodyText.text = rentText;
            else
                Debug.LogError("SpawnRentNotice: Couldn't find BodyText!");
        }
        else
            Debug.LogError("SpawnRentNotice: Couldn't find ImageComponent!");

        // Attach movement logic to RentNoticePrefab itself (since physics is on the parent now)
        if (rentImage.TryGetComponent<ImageObject>(out var mediaEntity))
            mediaEntity.SetUpSplinePath(splinePath); // Apply movement
        else
            Debug.LogError("SpawnRentNotice: RentNoticePrefab is missing Entity script!");
    }

    private void ShowHideRentNotices(bool show)
    {
        if(rentNotices.Count < 1)
            return;
        foreach(GameObject objectPiece in rentNotices)
        {
            if(objectPiece != null)
                objectPiece.SetActive(show);
            else
                Destroy(objectPiece);
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

        newspapers.Clear();
        newspaperPos = 0;

        ParseJson(json, "newspaperText");
        ParseJson(json, "grammerEngineText");
    }
    private void ParseJson(string json, string section)
    {
        //Our JSON deserialization needs to be significantly more complex to handle this.
        //Once all of the JSON is migrated over to the new format, it can be simplified
        try
        {
            //Load everything
            var jsonRoot = JObject.Parse(json);
            //Get just the newspaper text
            var newspaperTextArr = (JArray)jsonRoot[section];

            if (newspaperTextArr == null)
            {
                Debug.LogError($"Unable to find {section} data in JSON file. Stop breaking my stuff!.");
                return;
            }

            //Get the current day obj
            var currentPapers = newspaperTextArr.Where(item => (int)item["day"] == gameManager.gameData.GetCurrentDay())
                .Cast<JObject>().FirstOrDefault();

            if (currentPapers == null)
            {
                Debug.LogWarning($"Unable to find paper entries for day {gameManager.gameData.GetCurrentDay()} for {section}");
                return;
            }

            //Now go through and get the paper data
            var newsPaperArr = (JArray)currentPapers["newspapers"];

            for (var i = 0; i < newsPaperArr.Count; i++)
            {
                var newspaperObj = (JObject)newsPaperArr[i];
                var newspaper = new Entity.Newspaper
                {
                    date = newspaperObj["date"]?.ToString(),
                    hasHiddenImage = newspaperObj["hasHiddenImage"] != null && (bool)newspaperObj["hasHiddenImage"],
                    banWords = newspaperObj["banWords"]?.ToObject<string[]>() ?? Array.Empty<string>(),
                    censorWords = newspaperObj["censorWords"]?.ToObject<string[]>() ?? Array.Empty<string>(),
                    replaceWords = newspaperObj["replaceWords"]?.ToObject<string[][]>() ?? Array.Empty<string[]>(),
                };

                //Add all of the possibly complex objects
                ProcessField(newspaperObj["publisher"], out newspaper.publisher, out newspaper.publisherIsComplex);
                ProcessField(newspaperObj["title"], out newspaper.title, out newspaper.titleIsComplex);
                ProcessField(newspaperObj["front"], out newspaper.frontContent, out newspaper.frontIsComplex);
                ProcessField(newspaperObj["back"], out newspaper.backContent, out newspaper.backIsComplex);
                newspaper.CreateComplex();
                newspapers.Add(newspaper);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error while parsing newspaper JSON data: " + e.Message);
        }
        
        /// newspapers.ForEach(n => Debug.Log(n.GetTitle()));
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

    private void Reshuffle(Entity.Newspaper[] newspapers)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia
        for (int t = 0; t < newspapers.Length; t++)
        {
            Entity.Newspaper tmp = newspapers[t];
            int r = Random.Range(t, newspapers.Length);
            newspapers[t] = newspapers[r];
            newspapers[r] = tmp;
        }
    }

    private void ReadNextNewspaper()
    {
        if (newspaperPos < newspapers.Count)
        {
            currentNewspaper = newspapers[newspaperPos];
            newspaperPos++;
        }
        else
        {
            Debug.Log("End of newspapers.");
        }
}

    private void PassWordLists()
    {
        List<string> banWords = new List<string>();
        List<string> censorWords = new List<string>();
        List<string[]> replaceWords = new List<string[]>();

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
            foreach (string[] wordPair in newspaper.replaceWords)
            {
                replaceWords.Add(wordPair);
            }
        }

        gameManager.SetBanTargetWords(banWords.Distinct().ToArray());
        gameManager.SetCensorTargetWords(censorWords.Distinct().ToArray());
        gameManager.SetReplaceTargetWords(replaceWords.Distinct().ToArray());
    }

    void OnApplicationQuit()
    {
        quitting = true;
        spawnedMedia.Clear();
    }

    // EventManager for creating a new media object after one gets destroyed
    private void OnEnable()
    {
        EventManager.ShowHideRentNotices += ShowHideRentNotices;
        EventManager.OnMediaDestroyed += HandleMediaDestroyed;
        GameManager.OnGameRestart += StopSpawningOnRestart; // Listen for restarts
        EventManager.CameraZoomed += ShowHideObject;
    }

    private void OnDisable()
    {
        EventManager.ShowHideRentNotices -= ShowHideRentNotices;
        EventManager.OnMediaDestroyed -= HandleMediaDestroyed;
        GameManager.OnGameRestart -= StopSpawningOnRestart;
        EventManager.CameraZoomed -= ShowHideObject;
    }


    private void ShowHideObject(bool show)
    {
        foreach (GameObject gameObject in spawnedMedia)
        {
            if (gameObject == null)
            {
                //Destroy(gameObject);
            }
            else
            {
                //gameObject.SetActive(!show);
            }
        }
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
