using UnityEngine;

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

        ui.Close();
        SetTalkingState(false);
    }

    void SetTalkingState(bool talking)
    {
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
