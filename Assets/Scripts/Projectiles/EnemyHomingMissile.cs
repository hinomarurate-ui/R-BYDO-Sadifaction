using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyHomingMissile : MonoBehaviour
{
    [SerializeField] int damage = 10;
    [FormerlySerializedAs("upSpeed")]
    [SerializeField] float riseSpeed = 2f;
    [FormerlySerializedAs("upTime")]
    [SerializeField] float riseTime = 0.2f;
    [SerializeField] float turnSpeed = 180f;
    [SerializeField] float homingTime = 1.2f;
    [SerializeField] float straightTime = 0.1f;

    Rigidbody2D body;
    Transform player;
    Vector2 moveDirection = Vector2.right;
    float moveSpeed;
    float homingEndTime;
    bool isHoming;

    public void Init(Vector2 direction, float speedValue, float lifeTimeValue, int damageValue)
    {
        body = GetComponent<Rigidbody2D>();
        damage = damageValue;
        moveSpeed = speedValue;
        moveDirection = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
        player = FindPlayer();

        Destroy(gameObject, lifeTimeValue);
        if(body != null)
        {
            StartCoroutine(RiseThenLaunch(moveDirection));
        }
    }

    void Update()
    {
        RotateToVelocity();

        if(isHoming)
        {
            UpdateHomingMove();
        }
    }

    IEnumerator RiseThenLaunch(Vector2 launchDirection)
    {
        body.velocity = Vector2.one * riseSpeed;

        if(riseTime > 0f)
        {
            yield return new WaitForSeconds(riseTime);
        }

        moveDirection = -launchDirection.normalized;
        body.velocity = moveDirection * moveSpeed;

        if(straightTime > 0f)
        {
            yield return new WaitForSeconds(straightTime);
        }

        homingEndTime = Time.time + homingTime;
        isHoming = true;
    }

    void UpdateHomingMove()
    {
        if(body == null || Time.time > homingEndTime)
        {
            isHoming = false;
            return;
        }

        if(player != null)
        {
            Vector2 targetDirection = ((Vector2)player.position - body.position).normalized;
            moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, turnSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f);
        }

        body.velocity = moveDirection.normalized * moveSpeed;
    }

    Transform FindPlayer()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        return playerObject != null ? playerObject.transform : null;
    }

    void RotateToVelocity()
    {
        if(body == null || body.velocity.sqrMagnitude <= 0.001f)
        {
            return;
        }

        float angle = Mathf.Atan2(body.velocity.y, body.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponentInParent<PlayerController>();
        if(player == null)
        {
            return;
        }

        DamageRequest request = new DamageRequest(damage, gameObject, collision.bounds.center, 0f, 0f);
        DamageResult result = DamageUtility.ApplyDamage(player, request, false);
        if(result.Applied)
        {
            Destroy(gameObject);
        }
    }
}
