using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if(collision.CompareTag("Enemy"))
        {
            IDamageable enemy = collision.GetComponentInParent<IDamageable>();
            DamageRequest request = new DamageRequest(Damage, gameObject, collision.bounds.center, killShakePower, killShakeTime);
            if(enemy != null && enemy.TakeDamage(request).Killed)
            {
                ShakeScreen.Shake(killShakePower,killShakeTime);
            }
            Destroy(gameObject);
        }
    }
}
