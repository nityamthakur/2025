using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject fadingScreen;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image fadingImage;
    [SerializeField] private Image lightsOutImage;
    [SerializeField] private Image deskOverlayImage;
    [SerializeField] private Button menuButton;

    private bool mainMenuDone = false;

    void Awake()
    {
        menuButton.onClick.AddListener(() =>
        {
            EventManager.OpenOptionsMenu?.Invoke();
            EventManager.OptionsChanger?.Invoke("");
            EventManager.DisplayMenuButton?.Invoke(false);
            EventManager.PlaySound?.Invoke("switch1", true);
        });
        menuButton.gameObject.SetActive(false);
        
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
    }
    
   private void FadeInScreen()
    {
        StartCoroutine(FadeIn());
    }

    private void FadeOutScreen()
    {
        StartCoroutine(FadeOut());
    }

    private float fadeDuration = 1f; // Adjustable fade duration
    //private float waitTime = 1f; // Time to wait before fading back in
    private IEnumerator FadeIn()
    {
        yield return StartCoroutine(FadeImage(fadingImage, 1f, 0f, fadeDuration)); // fadeInOut goes to 100% opacity (Black Screen) 
        fadingImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeOut()
    {
        fadingImage.gameObject.SetActive(true);   
        yield return StartCoroutine(FadeImage(fadingImage, 0f, 1f, fadeDuration)); // fadeInOut goes to 100% opacity (Black Screen)    
    }

    private IEnumerator FadeImage(Image image, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        Color color = image.color;
        color.a = startAlpha;
        image.color = color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            image.color = color;
            yield return null;
        }

        color.a = endAlpha;
        image.color = color;
    }
    
    private void CanvasChanger(bool change)
    {
        if (canvas != null && !change)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
        }
        else if (canvas != null && change)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;            
        }
    }

    private void DisplayDeskOverLay(bool show)
    {
        Debug.Log("ShowDeskOverlay called");
        deskOverlayImage.gameObject.SetActive(show);
    }

    private void DisplayLightsOutImage(bool show)
    {
        //Debug.Log("ShowDeskOverlay called");
        lightsOutImage.gameObject.SetActive(show);   
    }

    private void DisplayMenuButton(bool active)
    {
        if(mainMenuDone)
        {
            menuButton.gameObject.SetActive(active);
        }
    }

    void OnEnable()
    {
        EventManager.FadeIn += FadeInScreen;
        EventManager.FadeOut += FadeOutScreen;
        EventManager.DisplayDeskOverlay += DisplayDeskOverLay;
        EventManager.DisplayLightsOutImage += DisplayLightsOutImage;
        EventManager.DisplayMenuButton += DisplayMenuButton;
        EventManager.CameraZoomed += CanvasChanger;
    }

    void OnDisable()
    {
        EventManager.FadeIn -= FadeInScreen;
        EventManager.FadeOut -= FadeOutScreen;
        EventManager.DisplayDeskOverlay -= DisplayDeskOverLay;
        EventManager.DisplayLightsOutImage -= DisplayLightsOutImage;
        EventManager.DisplayMenuButton -= DisplayMenuButton;
        EventManager.CameraZoomed -= CanvasChanger;
    }

}
