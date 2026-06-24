using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class DialogueView : MonoBehaviour
{
    [SerializeField] GameObject panelRoot;
    [FormerlySerializedAs("Name")]
    [SerializeField] TMP_Text speakerNameText;
    [FormerlySerializedAs("Talk")]
    [SerializeField] TMP_Text bodyText;
    [FormerlySerializedAs("continueObj")]
    [SerializeField] GameObject continueIndicator;
    [FormerlySerializedAs("charsPerSec")]
    [SerializeField] float charsPerSecond = 40f;

    Coroutine typingRoutine;

    public bool IsOpen { get; private set; }
    public bool IsTyping { get; private set; }

    void Awake()
    {
        Close();
    }

    public void Open()
    {
        IsOpen = true;
        if(panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
    }

    public void Close()
    {
        IsOpen = false;
        StopTypingRoutine();

        if(panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        SetText(speakerNameText, "");
        SetText(bodyText, "");
        SetContinueVisible(false);
        IsTyping = false;
    }

    public void SetLine(string speakerName, string body)
    {
        SetText(speakerNameText, speakerName);
        if(bodyText == null)
        {
            IsTyping = false;
            return;
        }

        bodyText.text = body;
        bodyText.maxVisibleCharacters = 0;
        bodyText.ForceMeshUpdate();

        StopTypingRoutine();
        typingRoutine = StartCoroutine(TypeByUnscaledTime());
    }

    public void SkipTyping()
    {
        if(!IsTyping || bodyText == null)
        {
            return;
        }

        StopTypingRoutine();
        bodyText.ForceMeshUpdate();
        bodyText.maxVisibleCharacters = bodyText.textInfo.characterCount;
        IsTyping = false;
        SetContinueVisible(true);
    }

    IEnumerator TypeByUnscaledTime()
    {
        IsTyping = true;
        SetContinueVisible(false);

        if(bodyText == null)
        {
            IsTyping = false;
            yield break;
        }

        bodyText.ForceMeshUpdate();
        int totalCharacters = bodyText.textInfo.characterCount;
        float visibleCharacters = 0f;
        int shownCharacters = 0;

        while(shownCharacters < totalCharacters)
        {
            visibleCharacters += Time.unscaledDeltaTime * charsPerSecond;
            int nextShown = Mathf.Clamp(Mathf.FloorToInt(visibleCharacters), 0, totalCharacters);
            if(nextShown != shownCharacters)
            {
                shownCharacters = nextShown;
                bodyText.maxVisibleCharacters = shownCharacters;
            }

            yield return null;
        }

        IsTyping = false;
        SetContinueVisible(true);
        typingRoutine = null;
    }

    void StopTypingRoutine()
    {
        if(typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }
    }

    void SetContinueVisible(bool visible)
    {
        if(continueIndicator != null)
        {
            continueIndicator.SetActive(visible);
        }
    }

    static void SetText(TMP_Text target, string value)
    {
        if(target != null)
        {
            target.text = value;
        }
    }
}
