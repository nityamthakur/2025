using UnityEngine;

public class CensorableObject : MonoBehaviour
{
    public bool isCensored = false; // Tracks if the object has been censored
    public GameObject censorTarget; // Assign the "CensorTarget" child here

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Called when the censor target is clicked
    public void CensorObject()
    {
        if (isCensored) return;

        isCensored = true;
        Debug.Log("Object censored!");

        // Example: Change object color to show it's censored
        spriteRenderer.color = Color.red;

        // Optionally hide the censor target after it's clicked
        censorTarget.SetActive(false);
    }
}