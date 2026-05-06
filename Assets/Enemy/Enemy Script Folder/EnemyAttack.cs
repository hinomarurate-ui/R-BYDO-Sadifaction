using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform shotPoint;
    [SerializeField] EnemyMove enemyMove;
    [SerializeField] Enemy enemy;
    [SerializeField]  float searchRange = 8f;
    [SerializeField]  float attackCooltime = 2f;
    [SerializeField]  float attackChargetime = 0.35f;
    [SerializeField]  float attackEndTime = 0.25f;
    [SerializeField]  float bulletSpeed = 6f;
    [SerializeField]  float bulletLifeTime = 3f;
    [SerializeField]  int bulletCount = 1;
    [SerializeField]  float bulletAngleSpace = 12f;
    [SerializeField]  float aimHeight = 0.5f;

    Transform player;
    Animator anim;
    Rigidbody2D rb;
    bool isAttacking;
    float lastAttackTime = -999f;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        enemyMove = GetComponent<EnemyMove>();
        enemy = GetComponent<Enemy>();
        
        GameObject playerObject = GameObject.FindWithTag("Player");
        if(playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(player == null || isAttacking)
        {
            return;
        }

        if(enemy == null || !enemy.isGround)
        {
            return;
        }

        if(Time.time < lastAttackTime + attackCooltime)
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        if(distance <= searchRange)
        {
            StartCoroutine(Attack());
        }
    }



IEnumerator Attack()
{
    isAttacking = true;
    lastAttackTime = Time.time;

    if(enemyMove != null)
    {
        enemyMove.enabled = false;
    }

    if(rb != null)
    {
        rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    if(anim != null)
    {
        anim.SetBool("Walk", false);
        anim.SetBool("Attack", true);
    }

    yield return new WaitForSeconds(attackChargetime);

    ShotFan();

    yield return new WaitForSeconds(attackEndTime);

    if(anim != null)
    {
        anim.SetBool("Attack", false);
    }

    if(enemyMove != null)
    {
        enemyMove.enabled = true;
    }

    isAttacking = false;
}

void ShotFan()
{
    if(bulletPrefab == null || player == null)
    {
        return;
    }

    Vector3 origin = shotPoint != null ? shotPoint.position : transform.position;
    Vector2 targetPos = player.position + Vector3.up * aimHeight;
    Vector2 baseDir = (targetPos - (Vector2)origin).normalized;

    for(int i = 0; i < bulletCount; i++)
    {
        float center = (bulletCount -1) *0.5f;
        float angle = (i - center) * bulletAngleSpace;
        Vector2 dir = (Vector2)(Quaternion.Euler(0f, 0f, angle) * baseDir);

        GameObject bullet = Instantiate(bulletPrefab, origin, Quaternion.identity);
        EnemyBullet enemyBullet = bullet.GetComponent<EnemyBullet>();
        if(enemyBullet != null)
        {
            enemyBullet.Init(dir, bulletSpeed, bulletLifeTime);
        }
    }
    
}
}