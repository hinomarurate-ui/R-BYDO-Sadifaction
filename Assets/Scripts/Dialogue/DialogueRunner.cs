using UnityEngine;
using UnityEngine.Serialization;

public class DialogueRunner : MonoBehaviour
{
    [FormerlySerializedAs("ui")]
    [SerializeField] DialogueView view;
    [SerializeField] KeyCode advanceKey = KeyCode.Space;
    [SerializeField] bool pauseGameWhileOpen = true;

    [Header("World Lock")]
    [FormerlySerializedAs("theWorld")]
    [SerializeField] MonoBehaviour[] disabledWhileOpen;
    [FormerlySerializedAs("Test")]
    [SerializeField] Dialogue startDialogue;

    Dialogue current;
    int currentLineIndex;
    float previousTimeScale = 1f;
    bool timeScalePaused;

    void Start()
    {
        Play(startDialogue);
    }

    void OnDisable()
    {
        if(current != null)
        {
            Stop();
        }
    }

    void Update()
    {
        if(view == null || !view.IsOpen || !Input.GetKeyDown(advanceKey))
        {
            return;
        }

        if(view.IsTyping)
        {
            view.SkipTyping();
            return;
        }

        Next();
    }

    public void Play(Dialogue sequence)
    {
        if(view == null || view.IsOpen || sequence == null || sequence.lines == null || sequence.lines.Length == 0)
        {
            return;
        }

        current = sequence;
        currentLineIndex = 0;
        view.Open();
        SetDialogueLock(true);
        ShowCurrentLine();
    }

    public void Stop()
    {
        current = null;
        currentLineIndex = 0;

        if(view != null)
        {
            view.Close();
        }

        SetDialogueLock(false);
    }

    void Next()
    {
        currentLineIndex++;
        if(current == null || currentLineIndex >= current.lines.Length)
        {
            Stop();
            return;
        }

        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        DialogueLine line = current.lines[currentLineIndex];
        view.SetLine(line.speakerName, line.body);
    }

    void SetDialogueLock(bool locked)
    {
        if(pauseGameWhileOpen)
        {
            if(locked && !timeScalePaused)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                timeScalePaused = true;
            }
            else if(!locked && timeScalePaused)
            {
                Time.timeScale = previousTimeScale;
                timeScalePaused = false;
            }
        }

        if(disabledWhileOpen == null)
        {
            return;
        }

        foreach(MonoBehaviour behaviour in disabledWhileOpen)
        {
            if(behaviour != null)
            {
                behaviour.enabled = !locked;
            }
        }
    }
}
