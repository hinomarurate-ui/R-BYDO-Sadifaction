using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScantController : MonoBehaviour, IEnemyMovement,IEnemyAttackPattern
{
    [Header("Hover Movement")]
    [SerializeField] float hoverMoveSpeed = 3.5f;
    [SerializeField] float horizontalRange = 6f;
    [SerializeField] float bobHeight = 0.35f;
    [SerializeField] float bobSpeed = 2f;

    [Header("Attack Cycle")]
    [SerializeField] float searchRange = 14f;
    [SerializeField] float openingDelay = 1.5f;
    [SerializeField] float attackCooldown = 2f;
    [SerializeField] float chargeTime = 0.65f;

    [Header("Shot")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform shotPoint;
    [SerializeField] int volleyCount = 1;
    [SerializeField] float volleyInterval = 1f;
    [SerializeField] int bulletsPerVolley = 1;
    [SerializeField] float bulletAngleSpacing = 14f;
    [SerializeField] float bulletSpeed = 10f;
    [SerializeField] float bulletLifeTime = 4f;
    [SerializeField] int bulletDamage = 50;
    [SerializeField] float aimHeight = 0.5f;
    [SerializeField] SpriteRenderer Blast;

    [Header("Dive Attack")]
    [SerializeField] float diveSpeed = 25f;
    [SerializeField] float returnSpeed = 15f;
    [SerializeField] float divePastDistance = 5f;
    [SerializeField] float diveRadius = 1f;
    [SerializeField] int diveDamage = 45;
    [SerializeField] float impactPause = 1f;
    [SerializeField] LayerMask playerLayers;

    [Header("Visual")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] bool spriteFacesLeft = true;
    [SerializeField] Animator an;
    [SerializeField] AudioClip Atak;
    [SerializeField] AudioClip Charge;
    [SerializeField] AudioClip Shot;
    [SerializeField] AudioSource As;

    readonly HashSet<IDamageable> diveHits = new HashSet<IDamageable>();

    EnemyController enemy;
    Transform player;
    Rigidbody2D body;
    Vector2 hoverCenter;
    Vector2 hoverTarget;
    float hoverOffsetX;
    float nextAttackTime;
    bool nextAttackIsDive;
    bool initialized;
    EnemyAttackKind selectedAttackKind = EnemyAttackKind.None;

    public EnemyAttackKind AttackKind { get { return selectedAttackKind; } }

    public void Initialize(EnemyController controller)
    {
        enemy = controller;
        player = controller != null ? controller.Target : null;
        body = GetComponent<Rigidbody2D>();

        if(spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if(!initialized)
        {
            initialized = true;
            hoverCenter = body != null ? body.position : (Vector2)transform.position;
            hoverTarget = hoverCenter;
            hoverOffsetX = 0f;
            nextAttackTime = Time.time + openingDelay;
        }
        
        
    }

    public bool Tick()
    {
        if(enemy == null || enemy.IsDead)
        {
            return false;
        }

        if(player == null)
        {
            player = enemy.Target;
        }

        float targetX = player != null ? player.position.x + hoverOffsetX : hoverCenter.x;
        targetX = Mathf.Clamp(targetX, hoverCenter.x - horizontalRange, hoverCenter.x + horizontalRange);
        float targetY = hoverCenter.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        hoverTarget = new Vector2(targetX,targetY);

        FacePlayer();

        Vector2 currentPosition = body != null ? body.position : (Vector2)transform.position;
        return Vector2.Distance(currentPosition, hoverTarget) > 0.05f;
    }

    public void FixedTick()
    {
        if(body == null)
        {
            return;
        }

        Vector2 difference = hoverTarget - body.position;
        body.velocity = Vector2.ClampMagnitude(difference * 3f, hoverMoveSpeed);
    }

    public void Stop()
    {
        if(body != null)
        {
            body.velocity = Vector2.zero;
        }
    }

    public bool CanAttack()
    {
        if(player == null || enemy == null || enemy.IsDead || Time.time < nextAttackTime)
        {
            selectedAttackKind = EnemyAttackKind.None;
            return false;
        }

        if(Vector2.Distance(transform.position, player.position) > searchRange)
        {
            selectedAttackKind = EnemyAttackKind.None;
            return false;
        }

        selectedAttackKind = nextAttackIsDive ? EnemyAttackKind.Melee : EnemyAttackKind.FanShot;
        return true;
    }

    public IEnumerator Attack()
    {
        nextAttackTime = float.PositiveInfinity;
        Stop();
        FacePlayer();

        if(selectedAttackKind == EnemyAttackKind.Melee)
        {
            yield return RunDiveAttack();
        }
        else
        {
            yield return RunShotAttack();
        }

        Stop();
        nextAttackIsDive = !nextAttackIsDive;
        nextAttackTime = Time.time + attackCooldown;
    }

    // Update is called once per frame
    public void Cancel()
    {
        Stop();
        selectedAttackKind = EnemyAttackKind.None;
        nextAttackTime = Time.time + attackCooldown;
    }

    IEnumerator RunShotAttack()
    {
        an.SetBool("ShotMode", true);
        an.SetBool("ShotStop", true);

        if(player == null)
        {
            yield break;
        }

        yield return MoveToPoint(new Vector2(transform.position.x, player.position.y + 0.35f), returnSpeed, false);
        


        if(chargeTime > 0f)
        {
            
            yield return new WaitForSeconds(chargeTime);
        }


        an.SetBool("ShotStop", false);
        //an.SetTrigger("Shot");
        As.PlayOneShot(Shot);
        int volleys = Mathf.Max(1, volleyCount);
        for(int i = 0; i < volleys; i++)
        {
            if(player == null)
            {
                yield break;
            }

            Vector3 origin = shotPoint != null ? shotPoint.position : transform.position;
            Vector2 direction = player.position.x > transform.position.x ? Vector2.right : Vector2.left;
            Vector3 Blastposition = Blast.transform.localPosition;
            Blastposition.x = player.position.x > transform.position.x ? Mathf.Abs(Blastposition.x): -Mathf.Abs(Blastposition.x);

            
            Blast.transform.localPosition = Blastposition;
            Blast.flipX = spriteRenderer.flipX;
            Blast.enabled = true;
            
            FanShotAttackPattern.Shoot(
                bulletPrefab,
                origin,
                direction,
                bulletsPerVolley,
                bulletAngleSpacing,
                bulletSpeed,
                bulletLifeTime,
                bulletDamage
            );

            yield return new WaitForSeconds(0.3f);
            Blast.enabled = false;


            if(i < volleys - 1 && volleyInterval > 0f)
            {
                yield return new WaitForSeconds(volleyInterval);
            }
        }
        an.SetBool("ShotMode", false);
    }

    IEnumerator RunDiveAttack()
    {
        if(player == null)
        {
            yield break;
        }

        an.SetBool("Attack", true);
        Vector2 returnPoint = body != null ? body.position : (Vector2)transform.position;
        Vector2 playerPoint = (Vector2)player.position + Vector2.up * 0.35f;
        diveHits.Clear();
        yield return MoveToPoint(new Vector2(returnPoint.x, playerPoint.y), returnSpeed, false);
        Vector2 diveTarget = new Vector2(playerPoint.x + (returnPoint.x < playerPoint.x ? divePastDistance : -divePastDistance), playerPoint.y);
        

        if(chargeTime > 0f)
        {
            yield return new WaitForSeconds(chargeTime);
        }
        As.PlayOneShot(Atak);

        yield return MoveToPoint(diveTarget, diveSpeed, true);

        if(impactPause > 0f)
        {
            yield return new WaitForSeconds(impactPause);
        }

        Vector2 oppositeHoverPoint = new Vector2(diveTarget.x, returnPoint.y);
        yield return MoveToPoint(oppositeHoverPoint, returnSpeed, false);
        hoverOffsetX = player != null ? diveTarget.x - player.position.x : 0f;
        hoverCenter = oppositeHoverPoint;
        hoverTarget = hoverCenter;
        an.SetBool("Attack", false);
    }

    IEnumerator MoveToPoint(Vector2 target, float speed, bool applyDamage)
    {
        const float arriveDistance = 0.08f;
        float safeSpeed = Mathf.Max(0.1f, speed);
        float remainingTime = Vector2.Distance(CurrentPosition(), target) / safeSpeed + 0.5f;

        while(Vector2.Distance(CurrentPosition(), target) > arriveDistance && remainingTime > 0f)
        {
            Vector2 nextPosition = Vector2.MoveTowards(CurrentPosition(), target, safeSpeed * Time.fixedDeltaTime);
            if(body != null)
            {
                body.MovePosition(nextPosition);
            }
            else
            {
                transform.position = nextPosition;
            }

            if(applyDamage)
            {
                ApplyDiveDamage(nextPosition);
            }

            remainingTime -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    void ApplyDiveDamage(Vector2 center)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, diveRadius, playerLayers);
        for(int i = 0; i < hits.Length; i++)
        {
            if(!DamageUtility.TryGetDamageable(hits[i], out IDamageable damageable) || diveHits.Contains(damageable))
            {
                continue;
            }

            diveHits.Add(damageable);
            DamageUtility.ApplyDamage(
                damageable,
                new DamageRequest(diveDamage, gameObject, hits[i].bounds.center, 0f, 0f),
                false
            );
        }
    }

    Vector2 CurrentPosition()
    {
        return body != null ? body.position : (Vector2)transform.position;
    }

    void FacePlayer()
    {
        if(spriteRenderer == null || player == null)
        {
            return;
        }

        bool playerIsRight = player.position.x > transform.position.x;
        spriteRenderer.flipX = spriteFacesLeft ? playerIsRight : !playerIsRight;
    }

    void OnDrawGizmoSelected()
    {
        Vector3 center = Application.isPlaying ? (Vector3)hoverCenter : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(center + Vector3.left * horizontalRange, center + Vector3.right * horizontalRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, diveRadius);
    }
}
