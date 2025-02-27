using System.Collections;
using UnityEngine;

public class PerformanceBuzzers : MonoBehaviour
{
    [SerializeField] private SpriteRenderer incorrectBuzzer;
    [SerializeField] private Color incorrectBuzzerColor;
    [SerializeField] private SpriteRenderer correctBuzzer;
    [SerializeField] private Color correctBuzzerColor;
    [SerializeField] private float flickerDuration = 2.0f;
    [SerializeField] private float flickerInterval = 0.2f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (incorrectBuzzer == null || correctBuzzer == null)
        {
            Debug.LogError("incorrectBuzzer or correctBuzzer is null in PerformanceBuzzers.");
        }
    }

    public void ShowIncorrectBuzzer()
    {
        StartCoroutine(ActivateBuzzer(incorrectBuzzer, incorrectBuzzerColor));
    }
    public void ShowCorrectBuzzer()
    {
        StartCoroutine(ActivateBuzzer(correctBuzzer, correctBuzzerColor));
    }

    IEnumerator ActivateBuzzer(SpriteRenderer buzzer, Color buzzerColor)
    {
        Color defaultColor = buzzer.color;
        float elapsedTime = 0.0f;

        while (elapsedTime < flickerDuration)
        {
            buzzer.color = buzzerColor;
            yield return new WaitForSeconds(flickerInterval);
            buzzer.color = defaultColor;
            yield return new WaitForSeconds(flickerInterval);
            elapsedTime += 2 * flickerInterval; // Update elapsed time
        }

        // Ensure the buzzer returns to its default color at the end
        buzzer.color = defaultColor;
    }
}
