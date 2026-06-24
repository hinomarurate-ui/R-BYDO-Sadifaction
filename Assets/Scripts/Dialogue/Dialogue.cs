using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Sequence")]
public class Dialogue : ScriptableObject
{
    public DialogueLine[] lines;
}

[Serializable]
public class DialogueLine
{
    [FormerlySerializedAs("name")]
    public string speakerName;
    [FormerlySerializedAs("text")]
    [TextArea(2, 5)]
    public string body;
}
