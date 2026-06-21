using UnityEngine;

public class EnemySensor
{
    GroundCheck ground;
    GakeChecker gake;

    public bool IsGrounded { get; private set; }
    public bool IsAtLedge { get; private set; }

    public void Initialize(Transform root, GroundCheck groundOverride, GakeChecker gakeOverride)
    {
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
