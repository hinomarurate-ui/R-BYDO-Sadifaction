using System.Collections.Generic;
using UnityEngine;

public class LedgeSensor : MonoBehaviour
{
    const string LedgeTag = "Gake";

    readonly HashSet<Collider2D> ledgeContacts = new HashSet<Collider2D>();

    public bool IsAtLedge()
    {
        ledgeContacts.RemoveWhere(collision => collision == null || !collision.enabled);
        return ledgeContacts.Count > 0;
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
}

