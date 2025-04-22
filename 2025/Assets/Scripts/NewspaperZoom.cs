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

    // Tool Overlay object
    public GameObject toolOverlayPrefab; 
    private GameObject toolOverlayObj, toolOverlayInstance;
    private Vector3 toolOverlayStartPos, toolOverlayEndPos;
    private GameObject UVLight;
    
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

        UVLight = gameManager.GetUVLightObj();
        if (UVLight == null)
        {
            Debug.LogError("UVLight not found in the scene!");
        }

        CreatePhone();
        CreateToolOverlay();
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
            gameManager.SetToolFunctionality(false);
            entityComponent.SetBlur(true);

            StartCoroutine(HideUIObject(phoneInstance, phoneStartPos, phoneEndPos));
            StartCoroutine(HideUIObject(toolOverlayInstance, toolOverlayStartPos, toolOverlayEndPos));
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
            gameManager.SetToolFunctionality(true);
            entityComponent.SetBlur(false);

            UpdatePhoneText();
            StartCoroutine(ShowUIObject(phoneInstance, phoneStartPos, phoneEndPos));
            StartCoroutine(ShowUIObject(toolOverlayInstance, toolOverlayStartPos, toolOverlayEndPos));
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

    private void CreateToolOverlay()
    {
        if (toolOverlayPrefab != null)
        {
            toolOverlayObj = Instantiate(toolOverlayPrefab);
            
            // Locate and attach the moveable pieces in ToolOverlayObj
            Transform toolOverlayTransform = toolOverlayObj.transform.Find("ToolOverlay");
            if (toolOverlayTransform != null)
                toolOverlayInstance = toolOverlayTransform.gameObject; // Fixed here
            else
                Debug.LogError("ToolOverlay is null inside toolOverlayPrefab.");

            // Set the position of the tool overlay
            toolOverlayStartPos = toolOverlayInstance.transform.position;
            toolOverlayEndPos = toolOverlayInstance.transform.position + new Vector3(0f, -1200f, 0f);
            toolOverlayInstance.transform.position = toolOverlayEndPos;

            gameManager.SetToolOverlayCreated(true);
        }
        else
        {
            Debug.LogError("Tool overlay prefab is missing!");
        }
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
            phoneEndPos = phoneInstance.transform.position + new Vector3(0f, -1200f, 0f);
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

    private IEnumerator ShowUIObject(GameObject instance, Vector3 startPos, Vector3 endPos)
    {
        if (instance == null)
        {
            Debug.LogError("Instance is null");
            yield break;
        }

        float elapsedTime = 0f;
        float duration = 0.3f;

        while (elapsedTime < duration)
        {
            instance.transform.position = Vector3.Lerp(endPos, startPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        instance.transform.position = startPos;
    }

    private IEnumerator HideUIObject(GameObject instance, Vector3 startPos, Vector3 endPos)
    {
        if (instance == null)
        {
            Debug.LogError("Instance is null");
            yield break;
        }

        float elapsedTime = 0f;
        float duration = 0.2f;

        while (elapsedTime < duration)
        {
            instance.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        instance.transform.position = endPos;
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
        Destroy(toolOverlayObj);
        toolOverlayObj = null;
    }
}