using System.Collections.Generic;
using UnityEngine;

public class LedgeSensor : MonoBehaviour
{
    const string LedgeTag = "Gake";

    readonly HashSet<Collider2D> ledgeContacts = new HashSet<Collider2D>();

    Collider2D ownCollider;

    void Awake()
    {
        ownCollider = GetComponent<Collider2D>();
    }

    public bool IsAtLedge()
    {
        ledgeContacts.RemoveWhere(collision => collision == null || !collision.enabled);
        return ledgeContacts.Count > 0 || RefreshCurrentOverlaps();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(IsLedgeCollider(collision))
        {
            ledgeContacts.Add(collision);
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if(IsLedgeCollider(collision))
        {
            ledgeContacts.Add(collision);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        ledgeContacts.Remove(collision);
    }

    bool IsLedgeCollider(Collider2D collision)
    {
        return collision != null && collision.CompareTag(LedgeTag);
    }

    bool RefreshCurrentOverlaps()
    {
        if(ownCollider == null)
        {
            return false;
        }

        Bounds bounds = ownCollider.bounds;
        Collider2D[] overlaps = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f);

        for(int i = 0; i < overlaps.Length; i++)
        {
            Collider2D collision = overlaps[i];
            if(collision == null || collision == ownCollider || !IsLedgeCollider(collision))
            {
                continue;
            }

            ledgeContacts.Add(collision);
            return true;
        }

        return false;
    }
}

