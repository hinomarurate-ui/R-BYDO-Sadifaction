using UnityEngine;

public class PlayerHomingMissile : MonoBehaviour
{
    [SerializeField] float speed = 20f;
    [SerializeField] float turnSpeed = 360f;
    [SerializeField] float lifeTime = 3f;
    [SerializeField] float targetRefreshInterval = 0.12f;
    [SerializeField] float homingTime = 1.4f;

    Rigidbody2D body;
    Transform target;
    Vector2 currentDirection = Vector2.right;
    LayerMask targetLayers;
    float targetRange;
    float facingDirection = 1f;
    float nextTargetRefreshTime;
    float homingEndTime;
    int damage = 3;
    float killShakePower = 0.3f;
    float killShakeTime = 0.2f;

    public void Init(
        Vector2 initialDirection,
        float facing,
        int damageValue,
        LayerMask seekLayers,
        float seekRange,
        float shakePower,
        float shakeTime)
    {
        body = GetComponent<Rigidbody2D>();
        facingDirection = facing == 0f ? 1f : Mathf.Sign(facing);
        currentDirection = initialDirection.sqrMagnitude > 0f ? initialDirection.normalized : new Vector2(facingDirection, 0f);
        damage = damageValue;
        targetLayers = seekLayers;
        targetRange = seekRange;
        killShakePower = shakePower;
        killShakeTime = shakeTime;
        homingEndTime = Time.time + homingTime;

        Destroy(gameObject, lifeTime);
        if(body == null)
        {
            return;
        }

        RefreshTargetIfNeeded();
        RotateTowardTarget();
        ApplyVelocity();
    }

    void FixedUpdate()
    {
        if(body == null)
        {
            return;
        }

        if(Time.time <= homingEndTime)
        {
            RefreshTargetIfNeeded();
            RotateTowardTarget();
        }

        ApplyVelocity();
    }

    void RefreshTargetIfNeeded()
    {
        if(Time.time < nextTargetRefreshTime && IsValidTarget(target))
        {
            return;
        }

        target = AcquireTarget();
        nextTargetRefreshTime = Time.time + targetRefreshInterval;
    }

    void RotateTowardTarget()
    {
        Vector2 desiredDirection = currentDirection;
        if(IsValidTarget(target))
        {
            desiredDirection = ((Vector2)target.position - body.position).normalized;
        }

        float maxRadiansDelta = turnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime;
        Vector3 rotatedDirection = Vector3.RotateTowards(currentDirection, desiredDirection, maxRadiansDelta, 0f);
        currentDirection = new Vector2(rotatedDirection.x, rotatedDirection.y).normalized;
        if(currentDirection.sqrMagnitude <= 0.0001f)
        {
            currentDirection = new Vector2(facingDirection, 0f);
        }
    }

    void ApplyVelocity()
    {
        body.velocity = currentDirection * speed;
        RotateToVelocity();
    }

    Transform AcquireTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, targetRange, targetLayers);
        Transform closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach(Collider2D hit in hits)
        {
            if(hit == null || !DamageUtility.TryGetDamageable(hit, out _))
            {
                continue;
            }

            if(!IsOnScreen(hit.transform.position))
            {
                continue;
            }

            Vector2 delta = hit.transform.position - transform.position;
            if(delta.x * facingDirection < -0.5f)
            {
                continue;
            }

            float sqrDistance = delta.sqrMagnitude;
            if(sqrDistance < closestDistance)
            {
                closestDistance = sqrDistance;
                closestTarget = hit.transform;
            }
        }

        return closestTarget;
    }

    bool IsValidTarget(Transform candidate)
    {
        return candidate != null && candidate.gameObject.activeInHierarchy && IsOnScreen(candidate.position);
    }

    bool IsOnScreen(Vector3 worldPosition)
    {
        Camera mainCamera = Camera.main;
        if(mainCamera == null)
        {
            return true;
        }

        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(worldPosition);
        return viewportPoint.z > 0f
            && viewportPoint.x >= 0f
            && viewportPoint.x <= 1f
            && viewportPoint.y >= 0f
            && viewportPoint.y <= 1f;
    }

    void RotateToVelocity()
    {
        if(body == null || body.velocity.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(body.velocity.y, body.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        DamageRequest request = new DamageRequest(damage, gameObject, collision.bounds.center, killShakePower, killShakeTime);
        if(DamageUtility.TryApplyDamage(collision, request, true, out _))
        {
            Destroy(gameObject);
        }
    }
}
