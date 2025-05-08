using UnityEngine;

public class GrayscaleToggle : MonoBehaviour
{
    [SerializeField] GameObject volume;
    void Start()
    {
        SetGrayscale(EventManager.IsGrayscale);
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
        if(volume != null)
            volume.SetActive(enable);
    }
}
