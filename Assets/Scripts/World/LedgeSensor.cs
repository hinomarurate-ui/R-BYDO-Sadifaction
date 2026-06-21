using System.Collections.Generic;
using UnityEngine;

public class LedgeSensor : MonoBehaviour
{
    const string GakeTag = "Gake";

    readonly HashSet<Collider2D> touchGake = new HashSet<Collider2D>();

    public bool IsGake()
    {
        touchGake.RemoveWhere(collision => collision == null || !collision.enabled);
        return touchGake.Count > 0;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(IsGakeCollider(collision))
        {
            touchGake.Add(collision);
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if(IsGakeCollider(collision))
        {
            touchGake.Add(collision);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        touchGake.Remove(collision);
    }

    bool IsGakeCollider(Collider2D collision)
    {
        return collision != null && collision.CompareTag(GakeTag);
    }
}

