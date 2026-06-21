using System.Collections.Generic;
using UnityEngine;

// 実装意図: 接地 trigger の接触候補から、本当に足元にある Ground だけを接地として返す。
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
        // 実装意図: 接触中 collider を集合で持ち、複数床や一時的な Exit 漏れに強くする。
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
        // 実装意図: 横や上から触れた Ground を接地扱いしないよう、高さで足元判定を絞る。
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
