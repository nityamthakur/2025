using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterText : MonoBehaviour
{
    private float textSpeed = 0.2f;
    private string message, helperMessage;
    private float lastSoundPlayed;
    private TextMeshProUGUI textObject;
    private Coroutine currentTypewriter;

    private void Awake()
    {
        textObject = transform.GetComponent<TextMeshProUGUI>();
        if(PlayerPrefs.HasKey("TextSpeed"))
            textSpeed = PlayerPrefs.GetFloat("TextSpeed", 1.0f);
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

            if (message[i] != ' ' && TextSoundLimiter())
            {
                EventManager.PlaySound?.Invoke("textBlip");
            }

            SetMessage(helperMessage);

            // At fastest speed, show text fully
            if(textSpeed == 0f)
                break;
            else
                yield return new WaitForSeconds(textSpeed);
        }
        SetMessage(message);
        currentTypewriter = null;
    }

    private bool TextSoundLimiter()
    {
        // Don't play sound if text speed is 0
        if (textSpeed == 0f)
            return false;

        float soundCoolDown = Mathf.Clamp(textSpeed, 0.06f, 0.2f);
        if (Time.time - lastSoundPlayed >= soundCoolDown)
        {
            lastSoundPlayed = Time.time;
            return true;
        }
        return false;
    }

    private void SetTextSpeed(float speed)
    {
        textSpeed = speed;
        Debug.Log(textSpeed);
    }

    private void OnEnable()
    {
        EventManager.SetTextSpeed += SetTextSpeed;       
    }

    private void OnDisable()
    {
        EventManager.SetTextSpeed -= SetTextSpeed;       
    }
}
