using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMissile : MonoBehaviour
{
    [SerializeField] int damage = 10;
    [SerializeField] float upSpeed = 2f; 
    [SerializeField] float upTime = 0.2f;
    [SerializeField] float turnSpeed = 180f;
    [SerializeField] float homingTime = 1.2f;
    [SerializeField] float straightTime = 0.1f;
    Rigidbody2D rb;
    Transform player;
    Vector2 moveDir;
    float shotSpeed;
    float homingEndTime;
    bool isHoming;
    
    void Update()
    {
        RotateToVelocity();

        if(isHoming)
        {
            HomingMove();
        }
    }

    public void Init(Vector2 dir, float speedValue, float lifeTimeValue, int damageValue)
    {
        rb = GetComponent<Rigidbody2D>();
        damage = damageValue;
        shotSpeed = speedValue;
        moveDir = dir.sqrMagnitude > 0f ? dir.normalized : Vector2.right;
        GameObject playerObject = GameObject.FindWithTag("Player");
        if(playerObject != null)
        {
            player = playerObject.transform;
        }

        Destroy(gameObject, lifeTimeValue);
        if(rb == null) return;

        StartCoroutine(MoveUpThenShot(moveDir, speedValue));
    }

    IEnumerator MoveUpThenShot(Vector2 dir, float speedValue)
    {
        if(rb == null) yield break;

        rb.velocity = new Vector2(1,1) * upSpeed;
        yield return new WaitForSeconds(upTime);
        moveDir = -dir.normalized;
        rb.velocity = moveDir * speedValue;
        yield return new WaitForSeconds(straightTime);
        homingEndTime = Time.time + homingTime;
        isHoming = true;
    }

    void HomingMove()
    {
        if(rb == null)
        {
            isHoming = false;
            return;
        }

        if(Time.time > homingEndTime)
        {
            isHoming = false;
            return;
        }

        if(player != null)
        {
            Vector2 targetDir = ((Vector2)player.position - rb.position).normalized;
            moveDir = Vector3.RotateTowards(moveDir, targetDir, turnSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f);
        }

        rb.velocity = moveDir.normalized * shotSpeed;
    }

    void RotateToVelocity()
    {
        if(rb == null || rb.velocity.sqrMagnitude <= 0.001f)
        {
            return;
        }

        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.GetComponentInParent<IDamageable>();
        if(target != null)
        {
            target.TakeDamage(new DamageRequest(damage, gameObject, collision.bounds.center, 0f, 0f));
            Destroy(gameObject);
        }
    }
}
