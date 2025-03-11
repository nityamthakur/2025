using System.Collections;
using UnityEngine;

public class PerformanceBuzzers : MonoBehaviour
{
    [SerializeField] private GameObject incorrectBuzzer;
    [SerializeField] private GameObject correctBuzzer;
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
        StartCoroutine(ActivateBuzzer(incorrectBuzzer));
    }
    public void ShowCorrectBuzzer()
    {
        StartCoroutine(ActivateBuzzer(correctBuzzer));
    }

    IEnumerator ActivateBuzzer(GameObject defBuzzer)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < flickerDuration)
        {
            defBuzzer.SetActive(false);
            yield return new WaitForSeconds(flickerInterval);
            defBuzzer.SetActive(true);
            yield return new WaitForSeconds(flickerInterval);
            elapsedTime += 2 * flickerInterval; // Update elapsed time
        }
        defBuzzer.SetActive(true);
    }
}
