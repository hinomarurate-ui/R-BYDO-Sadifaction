using UnityEngine;

// 実装意図: 会話シーケンスの再生・送り・終了時の一時停止をまとめて管理する入力窓口にする。
public class DialogueButton : MonoBehaviour
{
    [SerializeField] DialogueText ui;
    [SerializeField] KeyCode advanceKey = KeyCode.Space;
    [SerializeField] bool pauseGameWhileOpen = true;

    [Header("イベント中にザ・ワールド")]
    [SerializeField] MonoBehaviour[] theWorld;

    Dialogue current;
    int index;
    [SerializeField] Dialogue Test;

    void Start()
    {
        Play(Test);
    }

    void Update()
    {
        if(ui == null) return;
        if (!ui.isOpen) return;

        if(Input.GetKeyDown(advanceKey))
        {
            if(ui.isTyping)
            {
                ui.SkipTyping();
                return;
            }

            Next();
        }
    }

    public void Play(Dialogue seq)
    {
        // 実装意図: 空データや多重再生を無視して、会話 UI の状態破綻を避ける。
        if (ui == null) return;
        if (seq == null || seq.lines == null || seq.lines.Length == 0) return;
        if (ui.isOpen) return;

        current = seq;
        index = 0;

        ui.Open();
        SetTalkingState(true);

        ShowCurrent();
        
    }

    void ShowCurrent()
    {
        var line = current.lines[index];
        ui.Setline(line.name, line.text);
        
    }

    void Next()
    {
        index++;
        if(current == null || index >= current.lines.Length)
        {
            Stop();
            return;
        }
        ShowCurrent();
        
    }

    public void Stop()
    {
        current = null;
        index = 0;

        if(ui != null)
        {
            ui.Close();
        }
        SetTalkingState(false);
    }

    void SetTalkingState(bool talking)
    {
        // 実装意図: 会話中は時間停止と指定 component 停止でゲーム側の入力・移動を止める。
        if(pauseGameWhileOpen)
        Time.timeScale = talking ? 0f : 1f;

        if(theWorld != null)
        {
            foreach (var b in theWorld)
            {
                if(b) b.enabled = !talking;
            }
        }
        
    }


}
