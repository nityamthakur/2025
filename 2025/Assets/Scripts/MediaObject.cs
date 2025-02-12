using System;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class Entity : MonoBehaviour
{
    [SerializeField] private GameObject censorBoxPrefab;
    private TMP_Text textComponent;

    private void Start()
    {
        textComponent = GetComponentInChildren<TMP_Text>();
        if (textComponent == null)
        {
            Debug.LogError("No TextMeshPro component found!");
        }
        GenerateText();
        CreateCensorBoxes();
        ChangeMediaRotation(60);
    }

    private void GenerateText()
    {
        textComponent.text = 
        @"At the moment Brown Oil Conglomerate has no plans to begin drilling for oil at or near NewMerican rivers.
        In fact Brown Oil Conglomerate promises to prioritize drilling in less environmentally concerning locations.
        We are happy to reveal that our next operation will be within Bolivia’s rainforest.";
    
        // Force the text component to update its internal data
        textComponent.ForceMeshUpdate();
    }

    public void ChangeMediaRotation( int angleX )
    {
        transform.eulerAngles = new Vector3(
        transform.eulerAngles.x + angleX,
        transform.eulerAngles.y,
        transform.eulerAngles.z
    );}

    private void CreateCensorBoxes()
    {
        // Loop through each word in the text
        foreach (var wordInfo in textComponent.textInfo.wordInfo)
        {
            if (wordInfo.characterCount == 0) continue;

            string word = wordInfo.GetWord();
            bool isCensorable = false;
            // Find if the word contains any censor words
            foreach (string censorWord in GameManager.Instance.getCensorTargetWords())
            {
                // Split the censor word/phrase into individual words
                string[] splicedWord = censorWord.Split(' ');
                foreach (string individualWord in splicedWord)
                {
                    if (word.Contains(individualWord))
                    {
                        isCensorable = true;
                        break;
                    }
                }
            }
            if (!isCensorable) continue;

            // Gather the corners of the word
            var firstCharInfo = textComponent.textInfo.characterInfo[wordInfo.firstCharacterIndex];
            var lastCharInfo = textComponent.textInfo.characterInfo[wordInfo.lastCharacterIndex];
            var topLeft = textComponent.transform.TransformPoint(firstCharInfo.topLeft);
            var bottomRight = textComponent.transform.TransformPoint(lastCharInfo.bottomRight);
    
            // Create rectangle to block out the word
            var boxSize = new Vector2(Mathf.Abs(topLeft.x - bottomRight.x), 
                Mathf.Abs(topLeft.y - bottomRight.y));

            GameObject newCensorBox = Instantiate(censorBoxPrefab, transform);
            // Center the box around the word
            newCensorBox.transform.position = new Vector3((topLeft.x + bottomRight.x) / 2, (topLeft.y + bottomRight.y) / 2, 0);
            newCensorBox.transform.localScale = boxSize;

            GameManager.Instance.RegisterCensorTarget();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool draggable = GetComponent<Draggable>();
        Debug.Log($"Draggable? {draggable}");

        //Debug.Log($"Collided with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        if (collision.gameObject.CompareTag("DropBoxAccept"))
        {
            GameManager.Instance.EvaluatePlayerAccept();
            
            //Debug.Log("DropBox detected, destroying...");
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("DropBoxDestroy"))
        {
            GameManager.Instance.EvalutatePlayerDestroy();
            
            //Debug.Log("DropBox detected, destroying...");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        EventManager.OnMediaDestroyed?.Invoke(gameObject);
    }
}