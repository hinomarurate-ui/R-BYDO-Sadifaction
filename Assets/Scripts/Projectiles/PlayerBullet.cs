using UnityEngine;
using UnityEngine.Serialization;

public class PlayerBullet : MonoBehaviour
{
    [FormerlySerializedAs("Damage")]
    [SerializeField] int damage = 1;
    [FormerlySerializedAs("Range")]
    [SerializeField] float lifeTime = 2f;
    [FormerlySerializedAs("Speed")]
    [SerializeField] float speed = 1f;
    [FormerlySerializedAs("Dir")]
    [SerializeField] Vector2 direction = Vector2.right;

    [SerializeField] float killShakePower = 0.3f;
    [SerializeField] float killShakeTime = 0.2f;

    Rigidbody2D body;

    public void Init(Vector2 launchDirection, float launchSpeed)
    {
        body = GetComponent<Rigidbody2D>();
        direction = launchDirection.sqrMagnitude > 0f ? launchDirection.normalized : Vector2.right;
        speed = launchSpeed;

        if(body != null)
        {
            body.velocity = direction * speed;
        }

        RotateToDirection(direction);
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.CompareTag("Enemy"))
        {
            return;
        }

        DamageRequest request = new DamageRequest(damage, gameObject, collision.bounds.center, killShakePower, killShakeTime);
        DamageUtility.TryApplyDamage(collision, request, true, out _);
        Destroy(gameObject);
    }

    void RotateToDirection(Vector2 targetDirection)
    {
        float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
