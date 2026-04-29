using UnityEngine;

public class Missile : MonoBehaviour
{
    [SerializeField] float speed = 20f;
    [SerializeField] float turnSpeed = 360f;
    [SerializeField] float lifeTime = 3f;
    [SerializeField] float targetRefreshInterval = 0.12f;
    [SerializeField] float homingTime = 1.4f;

    Rigidbody2D rb;
    Transform target;
    Vector2 currentDirection = Vector2.right;
    LayerMask enemyLayers;
    float targetRange;
    float facingDir = 1f;
    float nextTargetRefreshTime = 0f;
    float homingEndTime;
    int damage = 3;
    float killShakePower = 0.3f;
    float killShakeTime = 0.2f;

    public void Init(Vector2 initialDirection, float facing, int damageValue, LayerMask targetLayers, float seekRange, float shakePower, float shakeTime)
    {
        rb = GetComponent<Rigidbody2D>();
        facingDir = facing == 0f ? 1f : Mathf.Sign(facing);
        currentDirection = initialDirection.sqrMagnitude > 0f ? initialDirection.normalized : new Vector2(facingDir, 0f);
        damage = damageValue;
        enemyLayers = targetLayers;
        targetRange = seekRange;
        killShakePower = shakePower;
        killShakeTime = shakeTime;
        homingEndTime = Time.time + homingTime;

        if (rb == null) return;
        Destroy(gameObject, lifeTime);

        if (Time.time >= nextTargetRefreshTime || !IsValidTarget(target))
        {
            target = AcquireTarget();
            nextTargetRefreshTime = Time.time + targetRefreshInterval;
        }

        Vector2 desiredDirection = currentDirection;
        if (IsValidTarget(target))
        {
            desiredDirection = ((Vector2)target.position - rb.position).normalized;
        }

        float maxRadiansDelta = turnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime;
        Vector3 rotatedDirection = Vector3.RotateTowards(currentDirection, desiredDirection, maxRadiansDelta, 0f);
        currentDirection = new Vector2(rotatedDirection.x, rotatedDirection.y).normalized;
        if(currentDirection.sqrMagnitude <= 0.0001f)
        {
            currentDirection = new Vector2(facingDir, 0f);
        }

        rb.velocity = currentDirection * speed;
        RotateToVelocity();
        
    }

    void FixedUpdate()
    {
        if(rb == null) return;

        if(Time.time <= homingEndTime)
        {
            
            if(Time.time >= nextTargetRefreshTime || !IsValidTarget(target))
            {
                
                target = AcquireTarget();
                nextTargetRefreshTime = Time.time + targetRefreshInterval;
            }

            Vector2 desiredDirection = currentDirection;
            if(IsValidTarget(target))
            {
                desiredDirection = ((Vector2)target.position - rb.position).normalized;
            }

            float maxRadiansDelta = turnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime;
            Vector3 rotatedDirection = Vector3.RotateTowards(currentDirection,desiredDirection,maxRadiansDelta,0f);
            currentDirection = new Vector2(rotatedDirection.x, rotatedDirection.y).normalized;
            if(currentDirection.sqrMagnitude <= 0.0001f)
            {
                currentDirection = new Vector2(facingDir, 0f);
            }
        }

        rb.velocity = currentDirection * speed;
        RotateToVelocity();
    }

    Transform AcquireTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, targetRange, enemyLayers);
        Transform closesTarget = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            EnemyHP enemyHP = hit.GetComponent<EnemyHP>();
            if (enemyHP == null) continue;
            if (!IsOnScreen(hit.transform.position)) continue;

            Vector2 delta = hit.transform.position - transform.position;
            if (delta.x * facingDir < -0.5f) continue;

            float sqrDistance = delta.sqrMagnitude;
            if(sqrDistance < closestDistance)
            {
                closestDistance =sqrDistance;
                closesTarget = hit.transform;
            }
        }

        return closesTarget;
    }

    bool IsValidTarget(Transform candidate)
    {
        return candidate != null && candidate.gameObject.activeInHierarchy && IsOnScreen(candidate.position);
    }

    bool IsOnScreen(Vector3 worldPosition)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return true;

        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(worldPosition);
        return viewportPoint.z > 0f
        && viewportPoint.x >= 0f && viewportPoint.x <= 1f
        && viewportPoint.y >= 0f && viewportPoint.y <= 1f;
    }

    void RotateToVelocity()
    {
        if (rb == null || rb.velocity.sqrMagnitude <= 0.0001f) return;

        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyHP enemy = collision.GetComponent<EnemyHP>();
        if (enemy == null) return;



        if(enemy.Damage(damage))
        {
           ShakeScreen.Shake(killShakePower,killShakeTime);
        }
        Destroy(gameObject);
    }

}
