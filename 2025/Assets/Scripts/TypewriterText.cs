using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterText : MonoBehaviour
{
    public float textSpeed = 0.1f;
    private string message;
    private string messageHelper;
    private bool clickMessageFinish = false; // For when the player clicks on the screen to get the message to appear immediatetly
    public TextMeshProUGUI textObject;
    private Coroutine currentTypewriter;

    public void setMessage(string text)
    {
        textObject.text = text;
    }

    public void typewriteMessage(string text)
    {
        message = "";
        setMessage(message);
        messageHelper = text;
        if(currentTypewriter != null)
        {
            StopCoroutine(currentTypewriter);
        }

        currentTypewriter = StartCoroutine(typeMessageHelper());
    }

    private IEnumerator typeMessageHelper()
    {
        foreach(char c in messageHelper)
        {
            message += c;
            EventManager.PlaySound?.Invoke("switch1"); // Change switch1 to something else
            setMessage(message);
            yield return new WaitForSeconds(textSpeed);
            if(clickMessageFinish)
                break;
        }
        yield return new WaitUntil(() => message == messageHelper);
    }
}
