using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GrayscaleToggle : MonoBehaviour
{
    public Material grayscaleMaterial;
    private Material originalMaterial;
    private Image image;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // Check if it's a UI Text or TextMeshPro
        if (GetComponent<TextMeshProUGUI>() != null || GetComponent<Text>() != null)
        {
            Debug.Log($"Skipping Grayscale for text object: {gameObject.name}");
            Destroy(this); // Remove script from text objects
            return;
        }

        image = GetComponent<Image>();
        if (image == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (image == null && spriteRenderer == null)
        {
            Debug.LogError("GrayscaleToggle script requires an Image or SpriteRenderer component on " + gameObject.name);
            return;
        }

        if(grayscaleMaterial == null)
        {
            grayscaleMaterial = Resources.Load<Material>("GrayScaleMaterial");
        }

        // Save the original material
        originalMaterial = image != null ? image.material : spriteRenderer.material;
    }


    void Start()
    {
        SetGrayscale(EventManager.IsGrayscale);
    }

    void OnEnable()
    {
        EventManager.ToggleGrayscale += SetGrayscale;
        // Re-apply grayscale in case the object was disabled and re-enabled
        SetGrayscale(EventManager.IsGrayscale);
    }

    void OnDisable()
    {
        EventManager.ToggleGrayscale -= SetGrayscale;
    }

    public void SetGrayscale(bool enable)
    {
        if (image != null)
        {
            image.material = enable ? grayscaleMaterial : originalMaterial;
            //Debug.Log($"Grayscale Applied to UI Image ({gameObject.name}): {enable}");
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.material = enable ? grayscaleMaterial : originalMaterial;
            //Debug.Log($"Grayscale Applied to Sprite ({gameObject.name}): {enable}");
        }
    }
}
