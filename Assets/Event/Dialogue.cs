using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Sequence")]
public class Dialogue : ScriptableObject
{
    public DialogueLine[] lines;
}

[System.Serializable]

public class DialogueLine
{
    public string name;
    [TextArea(2,5)] public string text;
}