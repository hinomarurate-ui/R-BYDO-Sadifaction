using System.Collections.Generic;
using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    const string GroundTag = "Ground";
    const float GroundTolerance = 0.05f;

    readonly HashSet<Collider2D> groundContacts = new HashSet<Collider2D>();

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

        Bounds bounds = ownCollider.bounds;
        groundContacts.RemoveWhere(collision => collision == null || !collision.enabled || !bounds.Intersects(collision.bounds));

        foreach(Collider2D collision in groundContacts)
        {
            if(IsGroundSurface(collision))
            {
                return true;
            }
        }

        return RefreshCurrentOverlaps();
    }

    bool IsGroundSurface(Collider2D collision)
    {
        if(collision == null || !collision.CompareTag(GroundTag))
        {
            return false;
        }

        Bounds checkerBounds = ownCollider.bounds;
        Vector2 probePosition = new Vector2(
            checkerBounds.center.x,
            checkerBounds.max.y + GroundTolerance);
        Vector2 surfacePosition = collision.ClosestPoint(probePosition);

        bool isWithinCheckerWidth = surfacePosition.x >= checkerBounds.min.x - GroundTolerance
            && surfacePosition.x <= checkerBounds.max.x + GroundTolerance;
        bool isNearCheckerHeight = surfacePosition.y >= checkerBounds.min.y - GroundTolerance
            && surfacePosition.y <= checkerBounds.max.y + GroundTolerance;
        return isWithinCheckerWidth && isNearCheckerHeight;


    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        groundContacts.Add(collision);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        groundContacts.Add(collision);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        groundContacts.Remove(collision);
    }

    bool RefreshCurrentOverlaps()
    {
        Bounds bounds = ownCollider.bounds;
        Collider2D[] overlaps = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f);

        for(int i = 0; i < overlaps.Length; i++)
        {
            Collider2D collision = overlaps[i];
            if(collision == null || collision == ownCollider)
            {
                continue;
            }

            if(IsGroundSurface(collision))
            {
                groundContacts.Add(collision);
                return true;
            }
        }

        return false;
    }
}

