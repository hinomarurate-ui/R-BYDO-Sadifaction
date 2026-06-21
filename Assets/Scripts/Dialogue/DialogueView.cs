using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueView : MonoBehaviour
{
    [SerializeField] GameObject panelRoot;
    [SerializeField] TMP_Text Name;
    [SerializeField] TMP_Text Talk;
    [SerializeField] GameObject continueObj;

    [SerializeField] float charsPerSec = 40f;

    Coroutine co;
    public bool isOpen;
    public bool isTyping;
 
    void Awake()
    {
        Close();
        
    }

    public void Open()
    {
        isOpen = true;
        if(panelRoot != null) panelRoot.SetActive(true);
    }

    public void Close()
    {
        isOpen = false;
        if(co != null) StopCoroutine(co);
        co = null;

        if(panelRoot != null) panelRoot.SetActive(false);
        if(Name != null) Name.text = "";
        if(Talk != null) Talk.text = "";
        if(continueObj != null) continueObj.SetActive(false);

        isTyping = false;
        
    }

    public void Setline(string name,string talk)
    {
        if(Name != null) Name.text = name;
        if(Talk == null)
        {
            isTyping = false;
            return;
        }

        Talk.text = talk;
        Talk.maxVisibleCharacters = 0;
        Talk.ForceMeshUpdate();

        if (co != null) StopCoroutine(co);
        co = StartCoroutine(TypeByUnscaledTime());

    }
    public void SkipTyping()
    {
        if(!isTyping) return;

        if(co != null) StopCoroutine(co);
        co = null;

        Talk.ForceMeshUpdate();
        Talk.maxVisibleCharacters = Talk.textInfo.characterCount;

        isTyping = false;
        if (continueObj) continueObj.SetActive(true);

    }

    IEnumerator TypeByUnscaledTime ()
    {
        isTyping = true;
        if (continueObj)continueObj.SetActive(false);

        if(Talk == null)
        {
            isTyping = false;
            yield break;
        }

        Talk.ForceMeshUpdate();
        int total = Talk.textInfo.characterCount;

        float t = 0f;
        int shown = 0;

        while(shown < total)
        {
            t += Time.unscaledDeltaTime * charsPerSec;
            int nextShown = Mathf.Clamp(Mathf.FloorToInt(t),0,total);
            
            if(nextShown != shown)
            {
                shown = nextShown;
                Talk.maxVisibleCharacters = shown;
            }

            yield return null;
        }
        isTyping = false;
         if(continueObj) continueObj.SetActive(true);

    }

    


}

