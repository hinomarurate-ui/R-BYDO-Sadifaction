using UnityEngine;

public class EnemySensor
{
    GroundSensor ground;
    LedgeSensor gake;

    public bool IsGrounded { get; private set; }
    public bool IsAtLedge { get; private set; }

    public void Initialize(Transform root, GroundSensor groundOverride, LedgeSensor gakeOverride)
    {
        ground = groundOverride != null ? groundOverride : FindChildComponent<GroundSensor>(root, "GroundChecker");
        gake = gakeOverride != null ? gakeOverride : FindChildComponent<LedgeSensor>(root, "LedgeSensor");
        if(gake == null)
        {
            gake = FindChildComponent<LedgeSensor>(root, "GakeChecker");
        }
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

