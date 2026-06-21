using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 実装意図: プレイヤー通常弾は IDamageable を叩く直進弾にし、敵 HP の具象クラスへ依存しない。
public class Bullet : MonoBehaviour
{
  
  public int Damage;
  [SerializeField]
  float Range;
    [SerializeField]
  float Speed =1;
    [SerializeField]
  Vector2 Dir;
  Rigidbody2D rb;

  float killShakePower = 0.3f;
  float killShakeTime = 0.2f;

    public void Init(Vector2 dir,float speed)
    {
        // 実装意図: 発射方向は Shot 側で決め、弾自身は速度・向き・寿命の初期化だけを担当する。
        rb = GetComponent<Rigidbody2D>();
        Dir = dir.sqrMagnitude > 0f ? dir.normalized : Vector2.right;
        Speed = speed;
        if(rb != null)
        {
            rb.velocity = Dir * Speed;
        }

        float angle = Mathf.Atan2(Dir.y,Dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle,Vector3.forward);
        Destroy(gameObject,Range);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 実装意図: Enemy タグの親階層から IDamageable を探し、敵 prefab 構成の違いを吸収する。
        if(collision.CompareTag("Enemy"))
        {
            IDamageable enemy = collision.GetComponentInParent<IDamageable>();
            if(enemy != null && enemy.TakeDamage(Damage))
            {
                ShakeScreen.Shake(killShakePower,killShakeTime);
            }
            Destroy(gameObject);
        }
    }
}
