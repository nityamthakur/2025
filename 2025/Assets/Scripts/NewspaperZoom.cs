using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;

public class NewspaperZoom : MonoBehaviour
{
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Vector3 previousPosition;
    private bool isZoomedIn = false;
    public bool IsZoomedIn
    {
        get { return isZoomedIn; }
        private set { isZoomedIn = value; }
    }    
    private Camera mainCamera;
    private Vector3 zoomPosition;
    private Vector3 zoomScale;

    public float zoomFactor = 1.25f;
    public float zoomSpeed = 0.2f;
    public bool canZoom = true;
    public bool stopZoom = true;
    public Entity entityComponent = null;
    public GameObject backOfNewspaper;
    
    // Phone object
    public GameObject phonePrefab;
    private GameObject phoneObj, phoneInstance;
    private Vector3 phoneStartPos, phoneEndPos;
    private TextMeshProUGUI phoneText;

    private Collider2D newspaperCollider;
    private Draggable draggableScript;
    private GameManager gameManager;

    void Start()
    {
        mainCamera = Camera.main;
        gameManager = FindFirstObjectByType<GameManager>();
        originalScale = transform.localScale;
        originalPosition = transform.position;
        newspaperCollider = GetComponent<Collider2D>();
        draggableScript = GetComponent<Draggable>();

        zoomPosition = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y + 2.25f, originalPosition.z);

        entityComponent = GetComponent<Entity>();
        if (entityComponent == null)
        {
            Debug.LogError("Entity component not found on Newspaper!");
        }

        if (backOfNewspaper == null)
        {
            Debug.LogError("Back of Newspaper not assigned!");
        }

        CreatePhone();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && canZoom && stopZoom)
        {
            canZoom = false;
            ToggleZoom();
        }
    }

    void ToggleZoom()
    {            
        EventManager.PlaySound?.Invoke("newspaperRustling");
        zoomFactor = 1.25f;
        zoomScale = originalScale * zoomFactor;

        if (isZoomedIn)
        {
            entityComponent.ObjectGravityOn(true);

            if (entityComponent)
                entityComponent.ChangeMediaRotation(60);

            StartCoroutine(SmoothTransition(previousPosition, originalScale));

            if (draggableScript != null)
                draggableScript.enabled = true;

            backOfNewspaper.SetActive(false);
            gameManager.SetCensorFunctionality(false);
            entityComponent.SetBlur(true);

            StartCoroutine(HidePhone());
        }
        else
        {
            entityComponent.ObjectGravityOn(false);

            if (entityComponent)
                entityComponent.ChangeMediaRotation(-60);

            StartCoroutine(SmoothTransition(zoomPosition, zoomScale));
            previousPosition = transform.position;

            if (draggableScript != null)
                draggableScript.enabled = false;

            backOfNewspaper.SetActive(true);
            gameManager.SetCensorFunctionality(true);
            entityComponent.SetBlur(false);

            UpdatePhoneText();
            StartCoroutine(ShowPhone());
        }
    }

    IEnumerator SmoothTransition(Vector3 targetPos, Vector3 targetScale)
    {
        newspaperCollider.enabled = false;

        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / zoomSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.position = targetPos;
        transform.localScale = targetScale;

        canZoom = true;
        isZoomedIn = !isZoomedIn;

        if (!isZoomedIn)
            newspaperCollider.enabled = true;
        else
            transform.position = zoomPosition;

    }

    private void CreatePhone()
    { 
        // Instantiate phone but keep it hidden initially
        if (phonePrefab != null)
        {
            phoneObj = Instantiate(phonePrefab);

            // Locate and attach the moveable phone pieces in PhoneObj
            Transform phoneInstanceTransform = phoneObj.transform.Find("PhoneObj");
            if (phoneInstanceTransform != null)
                phoneInstance = phoneInstanceTransform.gameObject; // Fixed here
            else
                Debug.LogError("PhoneObj is null inside phonePrefab.");

            // Locate the phone text component
            Transform phoneTextTransform = phoneInstanceTransform?.Find("PhoneTextDisplay");
            if (phoneTextTransform != null)
                phoneText = phoneTextTransform.GetComponent<TextMeshProUGUI>(); // Fixed here
            if (phoneText == null)
                Debug.LogError("PhoneText is null - Check that 'PhoneTextDisplay' has a TextMeshPro component.");

            phoneStartPos = phoneInstance.transform.position;
            phoneEndPos = phoneInstance.transform.position + new Vector3(0f, -1000f, 0f);
            phoneInstance.transform.position = phoneEndPos;
        }
        else
        {
            Debug.LogError("Phone prefab is missing!");
        }
    }
    private void UpdatePhoneText()
    {
        if (phoneText != null)
        {
            List<string> banWords = new List<string>(gameManager.GetBanTargetWords());
            List<string> censorWords = new List<string>(gameManager.GetCensorTargetWords());

            // Start with the Ban List
            string displayText = "<b>BAN LIST:</b>\n";
            foreach (string phrase in banWords)
            {
                displayText += phrase.Replace(" ", "\n") + "\n\n"; // Ensures multi-word phrases are split into separate lines
            }

            // Only show the Censor List from Day 2 onward
            if (gameManager.gameData.GetCurrentDay() > 1)
            {
                displayText += "<b>CENSOR LIST:</b>\n";
                foreach (string phrase in censorWords)
                {
                    displayText += phrase.Replace(" ", "\n") + "\n\n";
                }
            }

            phoneText.text = displayText;
        }
    }

    private IEnumerator ShowPhone()
    {
        float elapsedTime = 0f;
        float duration = 0.3f;

        while (elapsedTime < duration)
        {
            phoneInstance.transform.position = Vector3.Lerp(phoneEndPos, phoneStartPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        phoneInstance.transform.position = phoneStartPos;
    }

    private IEnumerator HidePhone()
    {
        float elapsedTime = 0f;
        float duration = 0.2f;

        while (elapsedTime < duration)
        {
            phoneInstance.transform.position = Vector3.Lerp(phoneStartPos, phoneEndPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        phoneInstance.transform.position = phoneEndPos;
    }

    public void PreventZoom()
    {
        stopZoom = false;
    }
    public void AllowZoom()
    {
        stopZoom = true;
    }
    private void OnDestroy()
    {
        Destroy(phoneObj);
        phoneObj = null;
    }
}