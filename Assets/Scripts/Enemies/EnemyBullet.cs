using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] int damage = 15;

    Rigidbody2D body;

    public void Init(Vector2 direction, float speed, float lifeTime, int damageValue)
    {
        body = GetComponent<Rigidbody2D>();
        damage = damageValue;

        if(body != null)
        {
            body.velocity = direction.normalized * speed;
        }

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        DamageRequest request = new DamageRequest(damage, gameObject, collision.bounds.center, 0f, 0f);
        if(DamageUtility.TryApplyDamage(collision, request, false, out _))
        {
            Destroy(gameObject);
        }
    }
}
