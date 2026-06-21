using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    const string GroundTag = "Ground";
    const float GroundTolerance = 0.05f;

    readonly HashSet<Collider2D> touchGround = new HashSet<Collider2D>();

    Collider2D ownCollider;

    void Awake()
    {
        ownCollider = GetComponent<Collider2D>();
    }

    public bool IsGround()
    {
        if(ownCollider == null)
        {
            return false;
        }

        touchGround.RemoveWhere(collision => collision == null || !collision.enabled);

        foreach(Collider2D collision in touchGround)
        {
            if(IsGroundSurface(collision))
            {
                return true;
            }
        }

        return false;
    }

    bool IsGroundSurface(Collider2D collision)
    {
        if(collision == null || !collision.CompareTag(GroundTag))
        {
            return false;
        }

        float checkerTopY = ownCollider.bounds.max.y;
        float groundTopY = collision.bounds.max.y;
        return groundTopY <= checkerTopY + GroundTolerance;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        touchGround.Add(collision);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        touchGround.Add(collision);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        touchGround.Remove(collision);
    }
}
