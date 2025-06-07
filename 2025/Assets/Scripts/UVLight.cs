using UnityEngine;
using System.Collections;
using System.Linq;

public class UVLight : MonoBehaviour
{
    [SerializeField] private Collider2D targetCollider;
    [SerializeField] private float blinkDuration = 0.25f;
    [SerializeField] private float[] radiusTiers = { 1.0f, 1.25f, 1.5f, 1.75f }; // index 0 = default, 1-3 = upgrades
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Collider2D uvLightCollider; // Reference to the collider (could be Circle or other type)
    private bool isOverlapping = false;
    private Color defColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //gameManager = FindObjectsByType<GameManager>(FindObjectsSortMode.None).FirstOrDefault();
        //spriteRenderer = GetComponent<SpriteRenderer>();
        //uvLightCollider = GetComponent<Collider2D>(); // Get whatever collider type is attached
        if (gameManager == null)
        {
            Debug.LogError("GameManager component not found on the UVLight object.");
            return;            
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on the UVLight object.");
            return;
        }

        if (uvLightCollider == null)
        {
            Debug.LogError("Collider2D component not found on the UVLight object.");
            return;
        }

        defColor = spriteRenderer.color;
    }

    void OnEnable()
    {
        ApplyUpgradeTier();
    }


    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0) return;

        // Move UV light to mouse position
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos;

        // Check if the player clicks on hidden image
        if (isOverlapping && Input.GetMouseButtonDown(0))
        {
            CheckClickOnTarget(mousePos);
        }
    }

    public void SetTargetCollider(Collider2D newTargetCollider)
    {
        targetCollider = newTargetCollider;
    }

    public void ApplyUpgradeTier()
    {
        int tier = gameManager.gameData.GetUVLightUpgradeTier();
        float radius = radiusTiers[tier];
        float xToYRatio = transform.localScale.y / transform.localScale.x;
        transform.localScale = new Vector3(radius, radius * xToYRatio, 1f);
        //Debug.Log($"UV Light set to tier {tier} radius: {radius}");
    }

    private bool AreBoundsOverlapping(Bounds a, Bounds b)
    {
        return a.min.x <= b.max.x && a.max.x >= b.min.x &&
            a.min.y <= b.max.y && a.max.y >= b.min.y;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Make sure targetCollider is not null before comparing
        if (targetCollider != null && other == targetCollider)
        {
            // Make sure both colliders are still valid
            if (uvLightCollider != null && targetCollider != null)
            {
                Bounds uvLightBounds = uvLightCollider.bounds;
                Bounds targetBounds = targetCollider.bounds;

                // Check if the UVLight bounds are contained within the target bounds
                isOverlapping = AreBoundsOverlapping(uvLightBounds, targetBounds);
            }
            else
            {
                isOverlapping = false;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (targetCollider != null && other == targetCollider)
        {
            isOverlapping = false;
        }
    }

    private void CheckClickOnTarget(Vector3 mousePosition)
    {
        if (targetCollider == null)
        {
            return;
        }

        // Perform a raycast at the mouse position to check if the targetCollider was clicked
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("HiddenImage"));

        if (hit.collider != null && hit.collider == targetCollider)
        {
            // Check if gameManager is available
            if (gameManager != null)
            {
                gameManager.SetTargetFound(true);
            }
            gameManager.DecrementLightSlider();

            StartCoroutine(ChangeLightColor());

            // Make sure hit.collider.gameObject is not null before accessing its components
            if (hit.collider != null && hit.collider.gameObject != null)
            {
                SpriteRenderer hitSprite = hit.collider.gameObject.GetComponent<SpriteRenderer>();
                if (hitSprite != null)
                {
                    hitSprite.maskInteraction = SpriteMaskInteraction.None;
                }

                BoxCollider2D hitCollider = hit.collider.gameObject.GetComponent<BoxCollider2D>();
                if (hitCollider != null)
                {
                    hitCollider.enabled = false;
                }
            }

            Debug.Log("Clicked on hidden image.");
        }
    }

    IEnumerator ChangeLightColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(Color.green.r, Color.green.g, Color.green.b, defColor.a);
            yield return new WaitForSeconds(blinkDuration);

            // Check if spriteRenderer is still valid
            if (spriteRenderer != null)
            {
                spriteRenderer.color = defColor;
            }
        }
    }
}