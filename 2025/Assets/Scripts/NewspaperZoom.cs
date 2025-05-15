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
    public GameObject toolOverlayUI;
    private Vector3 toolOverlayStartPos, toolOverlayEndPos;
    private GameObject UVLight;
    
    // Phone object
    private GameObject phoneOverlayUI;
    private Vector3 phoneOverlayStartPos, phoneOverlayEndPos;
    private TextMeshProUGUI phoneText;

    private Collider2D newspaperCollider;
    private Draggable draggableScript;
    private GameManager gameManager;
    private SelectedToolManager selectedToolManager;

    void Start()
    {
        mainCamera = Camera.main;
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager is not found in the scene.");
            return;
        }
        selectedToolManager = FindFirstObjectByType<SelectedToolManager>();
        if (selectedToolManager == null)
        {
            Debug.LogError("GameManager is not found in the scene.");
            return;
        }
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

        toolOverlayUI = gameManager.GetToolOverlayObj().transform.GetChild(0).gameObject;
        if (toolOverlayUI == null)
        {
            Debug.LogError("Tool Overlay UI Object not found!");
        }
        toolOverlayStartPos = toolOverlayUI.GetComponent<RectTransform>().anchoredPosition;
        toolOverlayEndPos = toolOverlayStartPos + new Vector3(0f, -1200f, 0f);

        Debug.Log(toolOverlayStartPos + " " + toolOverlayEndPos);

        phoneOverlayUI = gameManager.GetPhoneOverlayObj().transform.GetChild(0).gameObject;
        if (phoneOverlayUI == null)
        {
            Debug.LogError("Phone Overlay UI Object not found!");
        }
        phoneOverlayStartPos = phoneOverlayUI.GetComponent<RectTransform>().anchoredPosition;
        phoneOverlayEndPos = phoneOverlayStartPos + new Vector3(0f, -1200f, 0f);
        phoneText = phoneOverlayUI.GetComponentInChildren<TextMeshProUGUI>();

        Debug.Log(phoneOverlayStartPos + " " + phoneOverlayEndPos);

        UVLight = gameManager.GetUVLightObj();
        if (UVLight == null)
        {
            Debug.LogError("UVLight not found in the scene!");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && canZoom && stopZoom && Time.timeScale != 0)
        {
            canZoom = false;
            ToggleZoom();
        }
    }

    void ToggleZoom()
    {            
        EventManager.PlaySound?.Invoke("newspaperRustling", true);
        zoomFactor = 1.25f;
        zoomScale = originalScale * zoomFactor;
        GameObject hiddenImage = transform.Find("HiddenImage").gameObject;

        if (isZoomedIn)
        {
            entityComponent.ObjectGravityOn(true);

            if (entityComponent)
                entityComponent.ChangeMediaRotation(60);

            StartCoroutine(SmoothTransition(previousPosition, originalScale));

            if (draggableScript != null)
                draggableScript.enabled = true;

            backOfNewspaper.SetActive(false);
            selectedToolManager.SetToolFunctionality(false);
            gameManager.SetBanStampColliderActive(false);
            entityComponent.SetBlur(true);

            if (gameManager.UVLightTargetFound())
            {
                hiddenImage.SetActive(false);
            }   

            StartCoroutine(HideUIObject(phoneOverlayUI, phoneOverlayStartPos, phoneOverlayEndPos));
            StartCoroutine(HideUIObject(toolOverlayUI, toolOverlayStartPos, toolOverlayEndPos));
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
            selectedToolManager.SetToolFunctionality(true);
            gameManager.SetBanStampColliderActive(true);
            entityComponent.SetBlur(false);

            if (gameManager.UVLightTargetFound())
            {
                hiddenImage.SetActive(true);
            }

            UpdatePhoneText();
            StartCoroutine(ShowUIObject(phoneOverlayUI, phoneOverlayStartPos, phoneOverlayEndPos));
            StartCoroutine(ShowUIObject(toolOverlayUI, toolOverlayStartPos, toolOverlayEndPos));
        }
    }

    IEnumerator SmoothTransition(Vector3 targetPos, Vector3 targetScale)
    {
        //newspaperCollider.enabled = false;

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

        RectTransform rect = instance.GetComponent<RectTransform>();
        rect.anchoredPosition = endPos;
        instance.SetActive(true);

        float elapsedTime = 0f;
        float duration = 0.3f;

        while (elapsedTime < duration)
        {
            rect.anchoredPosition = Vector2.Lerp(endPos, startPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rect.anchoredPosition = startPos;
    }

    private IEnumerator HideUIObject(GameObject instance, Vector3 startPos, Vector3 endPos)
    {
        if (instance == null)
        {
            Debug.LogError("Instance is null");
            yield break;
        }

        RectTransform rect = instance.GetComponent<RectTransform>();
        float elapsedTime = 0f;
        float duration = 0.2f;

        while (elapsedTime < duration)
        {
            rect.anchoredPosition = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        instance.SetActive(false);
        rect.anchoredPosition = startPos;
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
        
    }
}