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

    void Start()
    {
       
    }

    public void Init(Vector2 dir,float speed)
    {
         rb = GetComponent<Rigidbody2D>();
        Dir = dir.normalized;
        Speed = speed;
        rb.velocity = Dir * Speed;
        Destroy(gameObject,Range);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            EnemyHP enemy = collision.GetComponent<EnemyHP>();
          if(enemy.Damage(Damage))
        {
            ShakeScreen.Shake(killShakePower,killShakeTime);
        }
            Destroy(gameObject);
        }
    }
}
