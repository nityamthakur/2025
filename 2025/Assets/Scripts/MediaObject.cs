using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Entity : MonoBehaviour
{
    [SerializeField] private GameObject censorBoxPrefab;
    [SerializeField] GameObject backOfNewspaper;
    [SerializeField] Material blurMaterial;
    [SerializeField] Material defaultMaterial;
    [SerializeField] float blurSize;
    [SerializeField] private GameObject hiddenImageSpawnBounds;
    private Newspaper newspaperData;
    private NewspaperZoom zoomComponent;
    private TMP_Text[] textComponents;
    private MediaSplinePath currSplinePath;
    private GameObject splinePrefab;
    private Draggable draggableScript;
    private GameManager gameManager;

    private Rigidbody2D rigidBody;
    private BoxCollider2D boxCollider;
    public bool beingDestroyed = false;
    private Vector2 screenBounds;
    private float playerHalfWidth;
    private float playerHalfHeight;    

    public void SetSplinePrefab(GameObject prefab)
    {
        splinePrefab = prefab;
    }

    private void Awake()
    {
        draggableScript = GetComponent<Draggable>(); // Get the Draggable script
        Canvas prefabCanvas = GetComponentInChildren<Canvas>();
        if (prefabCanvas != null)
        {
            prefabCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            prefabCanvas.worldCamera = Camera.main;
        }

    }

    private void Start()
    {
        textComponents = GetComponentsInChildren<TMP_Text>(true);

        if (textComponents == null)
        {
            Debug.LogError("No TextMeshPro components found!");
        }
        if (hiddenImageSpawnBounds == null)
        {
            Debug.LogError("Hidden image spawn bounds not assigned!");
        }
        gameManager = FindFirstObjectByType<GameManager>();
        zoomComponent = GetComponentInChildren<NewspaperZoom>();

        TryGetComponent<Rigidbody2D>(out var Urigid);
        rigidBody = Urigid;

        // Setting object boundaries to keep it inside the screen
        TryGetComponent<BoxCollider2D>(out var Ucollider);
        boxCollider = Ucollider;
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        playerHalfWidth = boxCollider.bounds.extents.x;
        playerHalfHeight = boxCollider.bounds.extents.y;

    }

    public void SetBlur(bool isBlurry)
    {
        if (isBlurry)
        {
            foreach (var textComponent in textComponents)
            {
                textComponent.fontSharedMaterial = blurMaterial;
                textComponent.fontSharedMaterial.SetFloat("_BlurSize", blurSize);
                textComponent.ForceMeshUpdate();
            }
        }
        else
        {
            foreach (var textComponent in textComponents)
            {
                textComponent.fontSharedMaterial = defaultMaterial;
                textComponent.ForceMeshUpdate();
            }
        }
    }

    public void PassNewspaperData(Newspaper newspaper)
    {
        if (newspaper == null)
        {
            Debug.LogError("Newspaper data is null.");
            return;
        }
        newspaperData = newspaper;

        // Text component order: 0 = title, 1 = date, 2 = publisher, 3 = front, 4 = back
        textComponents[0].text = newspaper.GetTitle();
        textComponents[1].text = newspaper.GetDate();
        textComponents[2].text = newspaper.GetPublisher();
        textComponents[3].text = newspaper.GetFront();
        textComponents[4].text = newspaper.GetBack();

        foreach (var textComponent in textComponents)
        {
            textComponent.fontSharedMaterial = blurMaterial;
            textComponent.fontSharedMaterial.SetFloat("_BlurSize", blurSize);
            textComponent.ForceMeshUpdate();
        }

        // Disable censoring on the first day
        if (gameManager.gameData.GetCurrentDay() != 0)
        {
            CreateCensorBoxes();
        }
        else backOfNewspaper.SetActive(false);

        ChangeMediaRotation(60);
        SetUpSplinePath(); 

        // Set the hidden image if it exists
        if (newspaper.hasHiddenImage)
        {
            StartCoroutine(DelayedInitializeHiddenImage());
        }
    }

    private IEnumerator DelayedInitializeHiddenImage()
    {
        yield return new WaitForSeconds(0.5f);

        InitializeHiddenImage();
    }

    private void InitializeHiddenImage()
    {
        GameObject hiddenImage = transform.Find("HiddenImage").gameObject;
        if (hiddenImage == null)
        {
            Debug.LogError("Hidden image object not found in the newspaper prefab.");
            return;
        }
        SpriteRenderer spawnBoundsRenderer = hiddenImageSpawnBounds.GetComponent<SpriteRenderer>();
        if (spawnBoundsRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on hiddenImageSpawnBounds.");
            return;
        }
        
        Bounds spawnBounds = spawnBoundsRenderer.localBounds;

        // Randomize the image's position
        float randomX = UnityEngine.Random.Range(spawnBounds.min.x, spawnBounds.max.x);
        float randomY = UnityEngine.Random.Range(spawnBounds.min.y, spawnBounds.max.y);

        Vector3 randomLocalPosition = new Vector3(randomX, randomY, 0);
        Vector3 randomWorldPosition = hiddenImageSpawnBounds.transform.TransformPoint(randomLocalPosition);

        hiddenImage.transform.position = randomWorldPosition;
        Vector3 localPosition = hiddenImage.transform.localPosition;
        localPosition.z = 0;
        hiddenImage.transform.localPosition = localPosition;

        hiddenImage.SetActive(true);
        gameManager.SetTargetExists(true);
        gameManager.SetUVLightTarget(hiddenImage);

        Debug.Log("Hidden image initialized at position: " + randomWorldPosition);
    }

    public void ChangeMediaRotation( int angleX )
    {
        transform.eulerAngles = new Vector3(
        transform.eulerAngles.x + angleX,
        transform.eulerAngles.y,
        transform.eulerAngles.z);
    }

    public void ObjectGravityOn(bool on)
    {
        if (rigidBody == null) return;

        if (on)
        {
            rigidBody.gravityScale = 3f; // Enable gravity
        }
        else
        {
            rigidBody.gravityScale = 0f; // Disable gravity
            rigidBody.linearVelocity = Vector2.zero; // Stop all movement
            rigidBody.angularVelocity = 0f; // Stop any rotation momentum
        }
    }

    private void CreateCensorBoxes()
    {
        for (int i = 0; i < textComponents.Length; i++)
        {
            TMP_Text textComponent = textComponents[i];
            bool isBack = i == textComponents.Length - 1;
            CreateBoxesForText(textComponent, isBack);
        }
        
    }

    private void CreateBoxesForText(TMP_Text textComponent, bool isBack)
    {
        if (isBack) 
            backOfNewspaper.SetActive(true);

        int curLineIndex = 0;
        int curWordIndex = 0;
        float prevLastCharX = 0f;
        
        // Loop through each word in the text
        foreach (var wordInfo in textComponent.textInfo.wordInfo)
        {
            if (wordInfo.characterCount == 0) continue;

            string word = wordInfo.GetWord();
            bool isCensorTarget = WordisCensorable(word);
            bool isReplaceTarget = WordisReplaceable(word);

            // Gather the corners of the word
            var firstCharInfo = textComponent.textInfo.characterInfo[wordInfo.firstCharacterIndex];
            var lastCharInfo = textComponent.textInfo.characterInfo[wordInfo.lastCharacterIndex];
            var topLeft = textComponent.transform.TransformPoint(firstCharInfo.topLeft);
            var bottomRight = textComponent.transform.TransformPoint(lastCharInfo.bottomRight);

            if (curWordIndex != 0) {
                topLeft.x = prevLastCharX;
            }
            prevLastCharX = bottomRight.x;
            
            // Create rectangle to block out the word
            var boxSize = new Vector2(Mathf.Abs(topLeft.x - bottomRight.x), 
                Mathf.Abs(topLeft.y - bottomRight.y));

            GameObject textComponentObj = textComponent.gameObject;
            GameObject newCensorBox = Instantiate(censorBoxPrefab, textComponentObj.transform);
            newCensorBox.GetComponent<CensorTarget>().InitializeCensorTarget(word, textComponent, wordInfo.firstCharacterIndex, wordInfo.characterCount, curLineIndex, curWordIndex, topLeft);
            textComponentObj.GetComponent<TextComponent>().AddCensorTarget(newCensorBox.GetComponent<CensorTarget>(), wordInfo.firstCharacterIndex);
            
            // Center the box around the word
            newCensorBox.transform.position = new Vector3((topLeft.x + bottomRight.x) / 2, (topLeft.y + bottomRight.y) / 2, 0);
            newCensorBox.transform.localScale = boxSize;

            curWordIndex++;
            if (curLineIndex < textComponent.textInfo.lineInfo.Length && curWordIndex >= textComponent.textInfo.lineInfo[curLineIndex].wordCount) {
                curLineIndex++;
                curWordIndex = 0;
            }

            if (isCensorTarget)
            {
                newCensorBox.GetComponent<CensorTarget>().SetToCensorTarget();
                gameManager.RegisterCensorTarget();
            }
            else if (isReplaceTarget)
            {
                newCensorBox.GetComponent<CensorTarget>().SetToReplaceTarget();
                gameManager.RegisterReplaceTarget();
            }
        }
        if (isBack)
            backOfNewspaper.SetActive(false);
    }

    private bool WordisCensorable(string word)
    {
        // Find if the word contains any censor words
        foreach (string censorWord in gameManager.GetCensorTargetWords())
        {
            // Split the censor word/phrase into individual words
            string[] splicedWord = censorWord.Split(' ');
            foreach (string individualWord in splicedWord)
            {
                if (word.Contains(individualWord))
                {
                    return true;
                }
            }
        }
        return false;
    }
    private bool WordisReplaceable(string word)
    {
        // Find if the word contains any replace words
        foreach (string[] replaceWord in gameManager.GetReplaceTargetWords())
        {
            // Split the replace word/phrase into individual words
            string[] splicedWord = replaceWord[0].Split(' ');
            foreach (string individualWord in splicedWord)
            {
                if (word.Contains(individualWord))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void UpdateCensorBoxes(CensorTarget cuttingRecipient, string replacementText)
    {
        TMP_Text textComponent = cuttingRecipient.GetTextComponent();
        int lineIndex = cuttingRecipient.GetWordLocation().Item1;
        int wordIndex = cuttingRecipient.GetWordLocation().Item2;
        
        int curLineIndex = 0;
        int curWordIndex = 0;
        float prevLastCharX = 0f;

        UpdateTextComponent(textComponent, replacementText, cuttingRecipient.GetTextCmpFirstIndex(), cuttingRecipient.GetWordCharCount());
        List<CensorTarget> targets = new List<CensorTarget>();
        int wordNum = 0;
        bool lineFound = false;
        
        // Loop through each word in the text
        foreach (var wordInfo in textComponent.textInfo.wordInfo)
        {
            if (wordInfo.characterCount == 0) continue;

            if (curLineIndex < lineIndex)
            {
                curWordIndex++;
                if (curLineIndex < textComponent.textInfo.lineInfo.Length && curWordIndex >= textComponent.textInfo.lineInfo[curLineIndex].wordCount) {
                    curLineIndex++;
                    curWordIndex = 0;
                }
                continue;
            }
            else if (!lineFound) 
            {
                lineFound = true;
                targets = textComponent.gameObject.GetComponent<TextComponent>().ClearCensorTargetIndices(wordInfo.firstCharacterIndex);
            }

            // Gather the corners of the word
            var firstCharInfo = textComponent.textInfo.characterInfo[wordInfo.firstCharacterIndex];
            var lastCharInfo = textComponent.textInfo.characterInfo[wordInfo.lastCharacterIndex];
            var topLeft = textComponent.transform.TransformPoint(firstCharInfo.topLeft);
            var bottomRight = textComponent.transform.TransformPoint(lastCharInfo.bottomRight);

            if (curWordIndex != 0) {
                topLeft.x = prevLastCharX;
            }
            prevLastCharX = bottomRight.x;
            
            // Create rectangle to block out the word
            var boxSize = new Vector2(Mathf.Abs(topLeft.x - bottomRight.x), 
                Mathf.Abs(topLeft.y - bottomRight.y));

            CensorTarget censorBox = targets[wordNum++];
            textComponent.gameObject.GetComponent<TextComponent>().AddCensorTarget(censorBox, wordInfo.firstCharacterIndex);

            censorBox.SetTextCmpFirstIndex(wordInfo.firstCharacterIndex);
            censorBox.SetWordCharCount(wordInfo.characterCount);
            
            // Center the box around the word
            censorBox.transform.position = new Vector3((topLeft.x + bottomRight.x) / 2, (topLeft.y + bottomRight.y) / 2, 0);
            Vector3 parentScale = censorBox.transform.parent.lossyScale;
            censorBox.transform.localScale = new Vector3(
                boxSize.x / parentScale.x,
                boxSize.y / parentScale.y,
                1
            );

            curWordIndex++;
            if (curLineIndex < textComponent.textInfo.lineInfo.Length && curWordIndex >= textComponent.textInfo.lineInfo[curLineIndex].wordCount) {
                curLineIndex++;
                curWordIndex = 0;
            }
        }
    }

    private void UpdateTextComponent(TMP_Text textComponent, string replacementText, int firstCharacterIndex, int characterCount)
    {
        if (textComponent == null)
        {
            Debug.LogError("Text component is null.");
            return;
        }

        // Remove any previous <nobr> tags for a clean search
        string taggedText = textComponent.text;
        // Remove all <nobr> tags to get the clean text
        string cleanText = taggedText.Replace("<nobr>", "").Replace("</nobr>", "");

        // Get the phrase to replace from the clean text
        if (firstCharacterIndex + characterCount > cleanText.Length)
        {
            Debug.LogError("Index out of range for replacement.");
            return;
        }
        string phraseToReplace = cleanText.Substring(firstCharacterIndex, characterCount);

        // Now, map the clean index to the tagged text index
        int realIndex = 0, cleanCount = 0;
        while (realIndex < taggedText.Length && cleanCount < firstCharacterIndex)
        {
            if (taggedText[realIndex] != '<')
                cleanCount++;
            else
            {
                int close = taggedText.IndexOf('>', realIndex);
                if (close == -1) break;
                realIndex = close;
            }
            realIndex++;
        }

        // Now, find the end index for replacement (skip tags in the substring)
        int replaceStart = realIndex;
        int charsToReplace = characterCount;
        int replaced = 0;
        int replaceEnd = replaceStart;
        while (replaceEnd < taggedText.Length && replaced < charsToReplace)
        {
            if (taggedText[replaceEnd] != '<')
                replaced++;
            else
            {
                int close = taggedText.IndexOf('>', replaceEnd);
                if (close == -1) break;
                replaceEnd = close;
            }
            replaceEnd++;
        }

        // Replace the substring (from replaceStart to replaceEnd) with the new <nobr>replacementText</nobr>
        string updatedText = taggedText.Substring(0, replaceStart)
            + "<nobr>" + replacementText + "</nobr>"
            + taggedText.Substring(replaceEnd);

        textComponent.text = updatedText;
        textComponent.ForceMeshUpdate();
    }


    private void SetUpSplinePath() 
    {
        // Create a new spline path
        if (splinePrefab == null)
        {
            Debug.LogError("Spline prefab not assigned correctly in Entity."); 
        }
        GameObject newSplinePath = Instantiate(splinePrefab);
        currSplinePath = newSplinePath.GetComponent<MediaSplinePath>();

        if (currSplinePath != null)
        {
            currSplinePath.EntranceMovement(transform);
            StartCoroutine(SpawnDelay(currSplinePath.GetDuration()));
        }
        else
        {
            Debug.LogError("MediaSplinePath component is missing on instantiated spline.");
        }
         
    }

    IEnumerator SpawnDelay(float duration)
    {
        draggableScript.enabled = false;
        zoomComponent.PreventZoom();
        yield return new WaitForSeconds(duration);
        zoomComponent.AllowZoom();
        draggableScript.enabled = true;
        ObjectGravityOn(true);
    }
    
    private bool isInsideTrigger = false;
    private Collider2D storedTrigger = null;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("DropBoxAccept") || other.gameObject.CompareTag("DropBoxDestroy"))
        {
            if(other.gameObject.CompareTag("DropBoxAccept"))
                EventManager.GlowingBoxShow?.Invoke("accept", true);
            if(other.gameObject.CompareTag("DropBoxDestroy"))
                EventManager.GlowingBoxShow?.Invoke("destroy", true);
            isInsideTrigger = true;
            storedTrigger = other;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == storedTrigger)
        {
            GameObject banStampCollider = transform.Find("BanStampCollider")?.gameObject;
            if (banStampCollider != null)
                gameManager.ResetBanStampCollider(banStampCollider);
            
            isInsideTrigger = false;
            storedTrigger = null;
            if(other.gameObject.CompareTag("DropBoxAccept"))
                EventManager.GlowingBoxShow?.Invoke("accept", false);
            if(other.gameObject.CompareTag("DropBoxDestroy"))
                EventManager.GlowingBoxShow?.Invoke("destroy", false);
        }
    }

    private void Update()
    {
        if (isInsideTrigger && storedTrigger != null && !draggableScript.IsDragging()) // Check if dragging has stopped inside trigger
        {
            if (storedTrigger.gameObject.CompareTag("DropBoxAccept"))
            {
                zoomComponent.PreventZoom();
                gameManager.EvaluatePlayerAccept(newspaperData.banWords);
                StartCoroutine(DestroyAfterExitMovement("Accept"));
            }
            else if (storedTrigger.gameObject.CompareTag("DropBoxDestroy"))
            {
                zoomComponent.PreventZoom();
                gameManager.EvalutatePlayerDestroy(newspaperData.banWords);
                StartCoroutine(DestroyAfterExitMovement("Destroy"));
            }

            // Prevent multiple triggers
            isInsideTrigger = false;
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
        EventManager.PlaySound?.Invoke("tossPaper", true);
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

        // Wait for the movement to complete (adjust time if needed)
        yield return new WaitForSeconds(1.5f);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (!GameManager.IsRestarting) // Prevent triggering OnMediaDestroyed when restarting
        {
            EventManager.OnMediaDestroyed?.Invoke(gameObject);
        }

        if (currSplinePath != null)
        {
            Destroy(currSplinePath.gameObject);
        }
    }

    [Serializable]
    public class Newspaper
    {
        public string publisher;
        public string title;
        public string date;
        
        [JsonIgnore] public string backContent;
        [JsonIgnore] public string frontContent;
        
        
        // Flags to determine if front/back are complex
        [JsonIgnore] public bool frontIsComplex;
        [JsonIgnore] public bool backIsComplex;
        [JsonIgnore] public bool publisherIsComplex;
        [JsonIgnore] public bool titleIsComplex;
        
        public string[] banWords;
        public string[] censorWords;
        public string[][] replaceWords;
        public bool hasHiddenImage;

        public string GetPublisher()
        {
            return publisherIsComplex ? FlattenGrammar(publisher) : publisher;
        }
        
        public string GetTitle()
        {
            return titleIsComplex ? FlattenGrammar(title) : title; 
        }
        
        public string GetFront()
        {
            return frontIsComplex ? FlattenGrammar(frontContent) : frontContent;
        }
        public string GetBack()
        {
            return backIsComplex ? FlattenGrammar(backContent) : backContent ;
        }
        public string GetDate()
        {
            return date;
        }


        private static string FlattenGrammar(string str)
        {
            var grammar = new TraceryNet.Grammar(str);
            return grammar.Flatten("#origin#");
        }
    }
}