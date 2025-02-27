using UnityEngine;
using TMPro;

public class UpdateFonts : MonoBehaviour
{
    public TMP_FontAsset newFont;

    void Start()
    {
        // Find all TextMeshPro objects in the scene
        TextMeshProUGUI[] textObjects = FindObjectsOfType<TextMeshProUGUI>();

        foreach (TextMeshProUGUI text in textObjects)
        {
            text.font = newFont;
        }
    }
}
