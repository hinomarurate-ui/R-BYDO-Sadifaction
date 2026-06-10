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
        if(rb != null && rb.velocity.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

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
        moveDir = dir.normalized;
        GameObject playerObject = GameObject.FindWithTag("Player");
        if(playerObject != null)
        {
            player = playerObject.transform;
        }

        StartCoroutine(MoveUpThenShot(dir, speedValue));

        Destroy(gameObject, lifeTimeValue);
    }

    IEnumerator MoveUpThenShot(Vector2 dir, float speedValue)
    {
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

    void OnTriggerEnter2D(Collider2D collision)
    {
        Tikuwa player = collision.GetComponentInParent<Tikuwa>();
        if(player != null)
        {
            player.Damage(damage);
            Destroy(gameObject);
        }
    }
}
