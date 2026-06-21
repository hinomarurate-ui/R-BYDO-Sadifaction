using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 実装意図: 敵の通常弾は生成側から速度と寿命だけ受け取り、プレイヤー命中時に消える軽い弾にする。
public class EnemyBullet : MonoBehaviour
{
    Rigidbody2D rb;
    // Start is called before the first frame update
    public void Init(Vector2 dir, float speed, float lifeTime)
    {
        // 実装意図: 方向決定は攻撃パターン側で行い、弾自身は直進と寿命管理だけを担当する。
        rb = GetComponent<Rigidbody2D>();

        if(rb != null)
        {
            rb.velocity = dir.normalized * speed;
        }

        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponentInParent<Tikuwa>() != null)
        {
            Destroy(gameObject);
        }
    }
}
