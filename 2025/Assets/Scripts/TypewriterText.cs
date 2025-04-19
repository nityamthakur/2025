using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterText : MonoBehaviour
{
    private float textSpeed = 0.1f;
    private string message, helperMessage;
    private TextMeshProUGUI textObject;
    private Coroutine currentTypewriter;

    private void Awake()
    {
        textObject = transform.GetComponent<TextMeshProUGUI>();
    }

    public bool MessageWriting()
    {
        return currentTypewriter != null;
    }

    public void SetMessage(string text)
    {
        textObject.text = text;
    }

    public void InstantMessage(string text)
    {
        StopCoroutine(currentTypewriter);
        currentTypewriter = null;
        textObject.text = text;
    }

    public void TypewriteMessage(string text)
    {
        message = text;
        SetMessage("");
        if(currentTypewriter != null)
        {
            StopCoroutine(currentTypewriter);
        }

        currentTypewriter = StartCoroutine(TypeMessageHelper());
    }

    private IEnumerator TypeMessageHelper()
    {
        for(int i = 0; i < message.Length; i++)
        {
            // Loop through the message, turning each letter from invisible to visible;
            helperMessage = message.Substring(0, i);
            helperMessage += "<color=#00000000>" + message.Substring(i) + "</color>"; 
            if (message[i] != ' ')
                EventManager.PlaySound?.Invoke("switch1");
            // Change switch1 to something else
            SetMessage(helperMessage);
            yield return new WaitForSeconds(textSpeed);
        }
        SetMessage(message);
        currentTypewriter = null;
    }
}
