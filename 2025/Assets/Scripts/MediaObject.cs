using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] private GameObject censorBoxPrefab;
    private Newspaper newspaperData;
    private TMP_Text textComponent;
    private MediaSplinePath currSplinePath;
    private GameObject splinePrefab;
    private Draggable draggableScript;

    public void SetSplinePrefab(GameObject prefab)
    {
        splinePrefab = prefab;
    }

    private void Start()
    {
        textComponent = GetComponentInChildren<TMP_Text>();
        if (textComponent == null)
        {
            Debug.LogError("No TextMeshPro component found!");
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

        textComponent.text = newspaper.body;

        textComponent.ForceMeshUpdate();

        // Disable censoring on the first day
        if (GameManager.Instance.GetCurrentDay() != 1)
        {
            CreateCensorBoxes();
        }
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
        int curLineIndex = 0;
        int curWordIndex = 0;
        float prevLastCharX = 0;
        
        // Loop through each word in the text
        foreach (var wordInfo in textComponent.textInfo.wordInfo)
        {
            if (wordInfo.characterCount == 0) continue;

            string word = wordInfo.GetWord();
            bool isCensorTarget = false;
            // Find if the word contains any censor words
            foreach (string censorWord in GameManager.Instance.GetCensorTargetWords())
            {
                // Split the censor word/phrase into individual words
                string[] splicedWord = censorWord.Split(' ');
                foreach (string individualWord in splicedWord)
                {
                    if (word.Contains(individualWord))
                    {
                        isCensorTarget = true;
                        break;
                    }
                }
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

            GameObject newCensorBox = Instantiate(censorBoxPrefab, transform);
            
            // Center the box around the word
            newCensorBox.transform.position = new Vector3((topLeft.x + bottomRight.x) / 2, (topLeft.y + bottomRight.y) / 2, 0);
            newCensorBox.transform.localScale = boxSize;

            curWordIndex++;
            if (curWordIndex >= textComponent.textInfo.lineInfo[curLineIndex].wordCount) {
                curLineIndex++;
                curWordIndex = 0;
            }

            if (isCensorTarget)
            {
                newCensorBox.GetComponent<CensorTarget>().SetToCensorTarget();
                GameManager.Instance.RegisterCensorTarget();
            }
        }
    }

    private void SetUpSplinePath() {
        draggableScript = GetComponent<Draggable>(); // Get the Draggable script
        draggableScript.enabled = false;        

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
        }
        else
        {
            Debug.LogError("MediaSplinePath component is missing on instantiated spline.");
        }
        draggableScript.enabled = true; 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("DropBoxAccept"))
        {
            GameManager.Instance.EvaluatePlayerAccept(newspaperData.banWords);
            StartCoroutine(DestroyAfterExitMovement("Accept"));

            NewspaperZoom zoomComponent = GetComponentInChildren<NewspaperZoom>();
            zoomComponent.preventZoom();
        }
        else if (collision.gameObject.CompareTag("DropBoxDestroy"))
        {
            GameManager.Instance.EvalutatePlayerDestroy(newspaperData.banWords);
            StartCoroutine(DestroyAfterExitMovement("Destroy"));

            NewspaperZoom zoomComponent = GetComponentInChildren<NewspaperZoom>();
            zoomComponent.preventZoom();
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
        public string body;
        public string[] banWords;
        public string[] censorWords;
    }
}