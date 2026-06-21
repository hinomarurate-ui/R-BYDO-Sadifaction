using System.Collections;
using TMPro;
using UnityEngine;

// 実装意図: 会話 UI の表示・タイプライター演出・スキップ処理だけを担当し、入力管理から分離する。
public class DialogueText : MonoBehaviour
{
    [SerializeField] GameObject panelRoot;
    [SerializeField] TMP_Text Name;
    [SerializeField] TMP_Text Talk;
    [SerializeField] GameObject continueObj;

    [SerializeField] float charsPerSec = 40f;

    Coroutine co;
    public bool isOpen;
    public bool isTyping;
 
    // Start is called before the first frame update
    void Awake()
    {
        Close();
        
    }

    // Update is called once per frame
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
        // 実装意図: 行切り替え時は前の coroutine を止め、常に最新行だけを表示進行させる。
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
        // 実装意図: Time.timeScale が 0 の会話中でも文字送りが進むよう unscaledDeltaTime を使う。
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
                //音鳴らしといて
            }

            yield return null;
        }
        isTyping = false;
         if(continueObj) continueObj.SetActive(true);

    }

    


}
