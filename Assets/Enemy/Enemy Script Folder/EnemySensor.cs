using UnityEngine;

// 実装意図: 接地・崖検知を EnemyController から分離し、センサー取得方法の変更をここだけに閉じ込める。
public class EnemySensor
{
    GroundCheck ground;
    GakeChecker gake;

    public bool IsGrounded { get; private set; }
    public bool IsAtLedge { get; private set; }

    public void Initialize(Transform root, GroundCheck groundOverride, GakeChecker gakeOverride)
    {
        // 実装意図: 既存 prefab の明示参照を優先しつつ、未設定なら子オブジェクト名から自動復旧する。
        ground = groundOverride != null ? groundOverride : FindChildComponent<GroundCheck>(root, "GroundChecker");
        gake = gakeOverride != null ? gakeOverride : FindChildComponent<GakeChecker>(root, "GakeChecker");
        Refresh();
    }

    public void Refresh()
    {
        IsGrounded = ground != null && ground.IsGround();
        IsAtLedge = gake != null && gake.IsGake();
    }

    static T FindChildComponent<T>(Transform root, string childName) where T : Component
    {
        if(root == null) return null;

        Transform child = root.Find(childName);
        return child != null ? child.GetComponent<T>() : null;
    }
}
