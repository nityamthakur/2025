using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NewspaperZoom : MonoBehaviour
{
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Vector3 previousPosition;
    private bool isZoomedIn = false;
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
    private GameObject phoneInstance;
    private TextMeshPro phoneText;

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

        // Instantiate phone but keep it hidden initially
        if (phonePrefab != null)
        {
            phoneInstance = Instantiate(phonePrefab, new Vector3(8f, -6f, 0), Quaternion.identity);

            // Correctly locate the child "PhoneTextDisplay" and get TMP component
            Transform phoneTextTransform = phoneInstance.transform.Find("PhoneTextDisplay");
            if (phoneTextTransform != null)
            {
                phoneText = phoneInstance.GetComponentInChildren<TextMeshPro>();
            }

            if (phoneText == null)
            {
                Debug.LogError("TextMeshProUGUI component not found in 'PhoneTextDisplay' child of the phone prefab!");
            }

            phoneInstance.SetActive(false);
        }
        else
        {
            Debug.LogError("Phone prefab is missing!");
        }
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
        zoomFactor = 1.25f;
        zoomScale = originalScale * zoomFactor;

        if (isZoomedIn)
        {
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

    System.Collections.IEnumerator SmoothTransition(Vector3 targetPos, Vector3 targetScale)
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
    }

    void UpdatePhoneText()
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

    IEnumerator ShowPhone()
    {
        phoneInstance.SetActive(true);
        Vector3 startPos = phoneInstance.transform.position;
        Vector3 targetPos = new Vector3(7f, -0.5f, 0);
        float elapsedTime = 0f;
        float duration = 0.3f;

        while (elapsedTime < duration)
        {
            phoneInstance.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        phoneInstance.transform.position = targetPos;
    }

    IEnumerator HidePhone()
    {
        Vector3 startPos = phoneInstance.transform.position;
        Vector3 targetPos = new Vector3(7f, -6f, 0);
        float elapsedTime = 0f;
        float duration = 0.2f;

        while (elapsedTime < duration)
        {
            phoneInstance.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        phoneInstance.transform.position = targetPos;
        phoneInstance.SetActive(false);
    }

    public void preventZoom()
    {
        stopZoom = false;
    }
}