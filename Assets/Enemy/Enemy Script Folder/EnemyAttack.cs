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
    [SerializeField]  float shotChargetime = 0.35f;
    [SerializeField]  float attackEndTime = 0.25f;
    [SerializeField]  float bulletSpeed = 6f;
    [SerializeField]  float bulletLifeTime = 3f;
    [SerializeField]  int bulletCount = 1;
    [SerializeField]  float bulletAngleSpace = 12f;
    [SerializeField]  float aimHeight = 0.5f;

    [Header("Melee Attack")]
    [SerializeField] bool useMeleeAttack;
    [SerializeField]  float meleeChargetime = 0.35f;
    [SerializeField] float meleeSearchRange = 1.6f;
    [SerializeField] float meleeRadius = 0.8f;
    [SerializeField] int meleeDamage = 15;
    [SerializeField] LayerMask  playerLayers;
    [SerializeField] Transform meleePoint;

    Transform player;
    Animator anim;
    Rigidbody2D rb;
    bool isAttacking;
    float lastAttackTime = -999f;
    bool isMeleeAttackNow;
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
        
        //float activeSearchRange = useMeleeAttack ? meleeSearchRange : searchRange;
        //if(distance <= activeSearchRange)
        //{
        //    StartCoroutine(Attack());
        //}
        
        float distance = Vector2.Distance(transform.position, player.position);
        if(useMeleeAttack && distance <= meleeSearchRange)
        {
            isMeleeAttackNow = true;
            StartCoroutine(Attack());
        }
        else if(distance <= searchRange)
        {
            isMeleeAttackNow = false;
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
        
    }

    

    if(isMeleeAttackNow)
    {
        anim.SetBool("Attack", true);
        yield return new WaitForSeconds(meleeChargetime);
        DoMeleeAttack();
    }
    else
    {
        anim.SetBool("Shot", true);
        yield return new WaitForSeconds(shotChargetime);
        ShotFan();
    }

    yield return new WaitForSeconds(attackEndTime);

    if(anim != null)
    {
        anim.SetBool("Attack", false);
        anim.SetBool("Shot", false);
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

void DoMeleeAttack()
{
    Vector3 origin = meleePoint != null ? meleePoint.position : transform.position;
    var hits = Physics2D.OverlapCircleAll(origin, meleeRadius, playerLayers);
    var damage = new HashSet<Tikuwa>();

    foreach(var h in hits)
    {
        if(h == null)
        {
            continue;
        }

        Tikuwa tikuwa = h.GetComponentInParent<Tikuwa>();
        if(tikuwa == null)
        {
            tikuwa = h.GetComponentInParent<Tikuwa>();
        }

        if(tikuwa == null || damage.Contains(tikuwa))
        {
            continue;
        }

        damage.Add(tikuwa);
        tikuwa.Damage(meleeDamage);
    }
}
}