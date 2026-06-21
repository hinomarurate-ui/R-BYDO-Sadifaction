using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Sequence")]
// 実装意図: 会話の内容を scene object から切り離し、ScriptableObject asset として再利用できるようにする。
public class Dialogue : ScriptableObject
{
    public DialogueLine[] lines;
}

[System.Serializable]

// 実装意図: 1 行分の話者名と本文を asset 内で編集しやすい単位にする。
public class DialogueLine
{
    public string name;
    [TextArea(2,5)] public string text;
}
