using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] private GameObject censorBoxPrefab;
    [SerializeField] private GameObject frontCensorBoxContainer;
    [SerializeField] private GameObject backCensorBoxContainer;
    [SerializeField] GameObject backOfNewspaper;
    [SerializeField] Material blurMaterial;
    [SerializeField] Material defaultMaterial;
    [SerializeField] float blurSize;
    private Newspaper newspaperData;
    private NewspaperZoom zoomComponent;
    private TMP_Text[] textComponents;
    private MediaSplinePath currSplinePath;
    private GameObject splinePrefab;
    private Draggable draggableScript;
    private GameManager gameManager;

    public void SetSplinePrefab(GameObject prefab)
    {
        splinePrefab = prefab;
    }

    private void Start()
    {
        textComponents = GetComponentsInChildren<TMP_Text>(true);
        if (textComponents == null)
        {
            Debug.LogError("No TextMeshPro components found!");
        }
        gameManager = FindFirstObjectByType<GameManager>();
        zoomComponent = GetComponent<NewspaperZoom>();
        draggableScript = GetComponent<Draggable>();
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
        textComponents[0].text = newspaper.title;
        textComponents[1].text = newspaper.date;
        textComponents[2].text = newspaper.publisher;
        textComponents[3].text = newspaper.front;
        textComponents[4].text = newspaper.back;

        foreach (var textComponent in textComponents)
        {
            textComponent.fontSharedMaterial = blurMaterial;
            textComponent.fontSharedMaterial.SetFloat("_BlurSize", blurSize);
            textComponent.ForceMeshUpdate();
        }

        // Disable censoring on the first day
        if (gameManager.gameData.GetCurrentDay() != 1)
        {
            CreateCensorBoxes();
        }
        else backOfNewspaper.SetActive(false);

        ChangeMediaRotation(60);
        SetUpSplinePath(); 
    }

    public void ChangeMediaRotation( int angleX )
    {
        transform.eulerAngles = new Vector3(
        transform.eulerAngles.x + angleX,
        transform.eulerAngles.y,
        transform.eulerAngles.z);
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
        GameObject censorBoxContainer;
        if (isBack) 
        {
            backOfNewspaper.SetActive(true);
            censorBoxContainer = backCensorBoxContainer;
        }
        else censorBoxContainer = frontCensorBoxContainer;

        int curLineIndex = 0;
        int curWordIndex = 0;
        float prevLastCharX = 0;
        
        // Loop through each word in the text
        foreach (var wordInfo in textComponent.textInfo.wordInfo)
        {
            if (wordInfo.characterCount == 0) continue;

            string word = wordInfo.GetWord();
            bool isCensorTarget = WordisCensorable(word);

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

            GameObject newCensorBox = Instantiate(censorBoxPrefab, censorBoxContainer.transform);
            
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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("DropBoxAccept"))
        {
            gameManager.EvaluatePlayerAccept(newspaperData.banWords);
            StartCoroutine(DestroyAfterExitMovement("Accept"));

            zoomComponent.PreventZoom();
        }
        else if (collision.gameObject.CompareTag("DropBoxDestroy"))
        {
            gameManager.EvalutatePlayerDestroy(newspaperData.banWords);
            StartCoroutine(DestroyAfterExitMovement("Destroy"));

            zoomComponent.PreventZoom();
        }
    }

    private IEnumerator DestroyAfterExitMovement(string box)
    {
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
        // Ensure the spline path is destroyed when media is destroyed
        if (currSplinePath != null)
        {
            Destroy(currSplinePath.gameObject);
        }
        
        EventManager.OnMediaDestroyed?.Invoke(gameObject);
    }

    [Serializable]
    public class Newspaper
    {
        public string publisher;
        public string title;
        public string date;
        public string front;
        public string back;
        public string[] banWords;
        public string[] censorWords;
    }
}