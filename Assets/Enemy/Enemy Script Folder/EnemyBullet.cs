using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    Rigidbody2D rb;
    // Start is called before the first frame update
    public void Init(Vector2 dir, float speed, float lifeTime)
    {
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
