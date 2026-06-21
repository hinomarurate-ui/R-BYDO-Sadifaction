using System.Collections.Generic;
using UnityEngine;

// 実装意図: 崖用 trigger との接触を保持し、敵移動が崖前ジャンプへ入る判断材料にする。
public class GakeChecker : MonoBehaviour
{
    const string GakeTag = "Gake";

    readonly HashSet<Collider2D> touchGake = new HashSet<Collider2D>();

    public bool IsGake()
    {
        // 実装意図: Destroy/disable 済み collider を掃除して、古い接触情報でジャンプし続ける問題を避ける。
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
