using UnityEngine;
using UnityEngine.UI;

public class GrayscaleToggle : MonoBehaviour
{
    public Material grayscaleMaterial;
    private Material originalMaterial;
    private Image image;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
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
        if (image != null)
            originalMaterial = image.material;
        else if (spriteRenderer != null)
            originalMaterial = spriteRenderer.material;
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
