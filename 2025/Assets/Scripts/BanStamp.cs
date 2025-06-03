using UnityEngine;

public class BanStamp : MonoBehaviour
{
    [SerializeField] private GameObject banStampCollider;
    private GameManager gameManager;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager is not found in the scene.");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0) return; 

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos;

        // Check if the player clicks on hidden image
        if (Input.GetMouseButtonDown(0))
        {
            CheckClickOnTarget(mousePos);
        }
    }

    public void BanStampColliderActive(bool isActive)
    {
        if (banStampCollider.transform.parent != transform)
        {
            banStampCollider.SetActive(isActive);
        }
    }

    private void CheckClickOnTarget(Vector3 mousePosition)
    {
        // Perform a raycast at the mouse position to check if the targetCollider was clicked
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("BanStamp"));

        if (hit.collider != null)
        {
            gameManager.SetBanStampPressed(false);

            banStampCollider.transform.SetParent(transform);
            banStampCollider.transform.position = mousePosition;
            banStampCollider.SetActive(false);

            gameManager.IncrementBanSlider();

            Debug.Log("Released ban stamp");
            return;
        }

        hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("Media"));

        if (hit.collider != null)
        {
            gameManager.SetBanStampPressed(true);

            gameManager.SetBanStampColliderParentToMediaObj(banStampCollider);
            banStampCollider.SetActive(true);

            gameManager.DecrementBanSlider();

            Debug.Log("Pressed ban stamp");
            return;
        }
        
    }
}
