using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovePstaff : MonoBehaviour
{
    [SerializeField]
    float MoveSpeed;
    Rigidbody2D rb;
    [SerializeField]
    float Dirx;
    [SerializeField]
    Enemy enemy;

    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform shotPoint;
    [SerializeField]  float searchRange = 8f;
    [SerializeField]  float attackCooltime = 2f;
    [SerializeField]  float shotChargetime = 0.35f;
    [SerializeField]  float attackEndTime = 0.25f;
    [SerializeField]  float bulletSpeed = 6f;
    [SerializeField]  float bulletLifeTime = 3f;
    [SerializeField]  int bulletCount = 1;
    [SerializeField]  float bulletAngleSpace = 12f;
    [SerializeField]  float aimHeight = 0.5f;

    private Animator anim = null;
    Transform player;
    bool isAttacking;
    float lastAttackTime = -999f;
    

    public void Init(EnemyStatus ES)
    {
        MoveSpeed = ES.MoveSpeed;
    }

    void Start()
    {    
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        GameObject playerObject = GameObject.FindWithTag("Player");
        if(playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    public void MoveX(float Dirx)
    {
        //anim.SetBool("Missile", false);
        anim.SetBool("Walk", true);
        rb.velocity = new Vector2(Dirx * MoveSpeed, rb.velocity.y);
    }


    void Update()
    {
        if(isAttacking)
        {
            return;
        }
    float distance = Vector2.Distance(transform.position, player.position);
    if (distance < searchRange)
    {
        Dirx = 0;
        StartCoroutine(Attack());
    }    
        
        
    }

    void FixedUpdate()
    {
        if(gameObject.layer == LayerMask.NameToLayer("Corpse"))
        {
            return;
        }

        if(isAttacking)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            
        }
        MoveX(Dirx);

    }

    IEnumerator Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;


        if(rb != null)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        if(anim != null)
        {
            anim.SetBool("Walk", false);
        
        }
        anim.SetBool("Missile", true);
        yield return new WaitForSeconds(shotChargetime);
        ShotFan();


        yield return new WaitForSeconds(attackEndTime);
        anim.SetBool("Shot", false);
        anim.SetTrigger("MissileFinish");
        anim.SetBool("Missile", false);

        yield return new WaitForSeconds(attackCooltime);
        
        isAttacking = false;

    }

    void ShotFan()
    {

    Vector3 origin = shotPoint != null ? shotPoint.position : transform.position;
    Vector2 baseDir = (player.position - origin).normalized;
    anim.SetBool("Shot", true);

    for(int i = 0; i < bulletCount; i++)
    {
        float center = (bulletCount -1) *0.5f;
        float angle = (i - center) * bulletAngleSpace;
        Vector2 dir = (Vector2)(Quaternion.Euler(0f, 0f, angle) * baseDir);

        /*GameObject bullet = Instantiate(bulletPrefab, origin, Quaternion.identity);
        EnemyBullet enemyBullet = bullet.GetComponent<EnemyBullet>();
        if(enemyBullet != null)
        {
            enemyBullet.Init(dir, bulletSpeed, bulletLifeTime);
        }*/
    }

    }

}

