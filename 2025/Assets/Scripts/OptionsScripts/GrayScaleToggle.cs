using UnityEngine;

public class GrayscaleToggle : MonoBehaviour
{
    [SerializeField] GameObject volume;
    private bool grayscaleOn = false;
    void Awake()
    {
        if (!PlayerPrefs.HasKey("GrayState"))
            PlayerPrefs.SetInt("GrayState", 0);

        grayscaleOn = PlayerPrefs.GetInt("GrayState", 0) == 1;
        SetGrayscale(grayscaleOn);
    }

    void OnEnable()
    {
        EventManager.ToggleGrayscale += SetGrayscale;
    }

    void OnDisable()
    {
        EventManager.ToggleGrayscale -= SetGrayscale;
    }

    public void SetGrayscale(bool enable)
    {
        if (volume != null)
            volume.SetActive(enable);

        EventManager.IsGrayscale = enable;
    }
}
