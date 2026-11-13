using System.Collections;
using UnityEngine;

public class CancerController : EnemyBehaviourBase, IEnemyMovement, IEnemyAttackPattern, IEnemyRoutine
{
    [SerializeField] float patrolSpeed = 1.5f;
    [SerializeField] float patrolHalfWidth = 3f;
    [SerializeField] float chaseSpeed = 4.5f;
    [SerializeField] float searchRange = 10f;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform shotPoint;
    [SerializeField] float tackleSpeed = 12f;
    [SerializeField] int tackleDamage = 25;
    [SerializeField] LayerMask playerLayers = 1 << 7;

    [Header("Visual")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] bool spriteFacesLeft = true;
    [SerializeField] Animator an;
    [SerializeField] AudioClip Atak;
    [SerializeField] AudioClip Charge;
    [SerializeField] AudioClip Shot;
    [SerializeField] AudioSource As;

    PlayerController player;
    Vector2 patrolCenter;
    float moveTargetX;
    float moveSpeed;
    float facingX = -1f;
    float waitTime;
    float nextAttackTime;
    float nextTackleTime;
    bool foundPlayer;
    bool attacking;
    int attackVersion;

    Coroutine turnRoutine;

    public EnemyAttackKind AttackKind { get; private set;}

    protected override void OnInitialized()
    {
        if(an == null) an = CachedAnimator;
        if(spriteRenderer == null) spriteRenderer = CachedSpriteRenderer;
        if(As == null) As = CachedAudioSource;
        if(shotPoint == null) shotPoint = transform.Find("ShotPoint");

        ResetRuntimeState();
    }

    protected override void OnBehaviourEnabled()
    {
        if(Enemy != null && !Enemy.IsDead)
        {
            ResetRuntimeState();
        }
    }

    protected override void OnBehaviourDisabled()
    {
        Stop();
    }

    void ResetRuntimeState()
    {
        if(Body == null)
        {
            return;
        }

        Body.gravityScale = 0f;
        patrolCenter = Body.position;
        moveTargetX = Body.position.x;
        foundPlayer = false;
        waitTime = Time.time;
        player = Target != null ? Target.GetComponent<PlayerController>() : null;
        Face(-1f);
    }

    void IEnemyRoutine.Tick()
    {
        if(!enabled || attacking) return;
        if(player == null)
        {
            player = Target != null ? Target.GetComponent<PlayerController>() : null;
        }
        float range = foundPlayer ? searchRange + 4f : searchRange;
        bool visible = player != null && player.gameObject.activeInHierarchy && player.CurrentHP > 0
            && Vector2.Distance(Body.position, player.transform.position) <= range;
            visible = visible && (player.transform.position.x - Body.position.x) * facingX > 0f;
        if(foundPlayer || !visible) return;

        foundPlayer = visible;
        Stop();
        waitTime = Time.time + 0.65f;
        if(foundPlayer)
        {
            Face(player.transform.position.x - Body.position.x);
            if(an != null) an.SetTrigger("Find");
            nextAttackTime = waitTime + 2f;
        }
        else patrolCenter = Body.position;
    }

    IEnumerator WaitAnim(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        turnRoutine = null;
        if(!enabled || foundPlayer || Enemy == null || !Enemy.CanAct) yield break;
        Face(-facingX);
        if(an != null) an.SetBool("Search", false);
    }

    void LateUpdate()
    {
        if(an != null && Enemy != null) an.SetBool("Damage", Enemy.State == EnemyState.HitStun);
    }

    public bool Tick()
    {
        moveSpeed = 0f;
        if(!enabled || Time.time < waitTime|| turnRoutine != null) return false;
        if(an != null) an.SetBool("Search", false);
        if(foundPlayer)
        {
            if(player == null || !player.gameObject.activeInHierarchy || player.CurrentHP <= 0) return false;
            float difference = player.transform.position.x - Body.position.x;
            Face(difference);
            if(Mathf.Abs(difference) <= 0.75f) return false;
            moveTargetX = player.transform.position.x - facingX * 0.75f;
            moveSpeed = chaseSpeed;
        }
        else
        {
            moveTargetX = patrolCenter.x + facingX * patrolHalfWidth;
            if(Mathf.Abs(moveTargetX - Body.position.x) < 0.02f)
            {
                if(an != null)an.SetBool("Search", true);
                waitTime = Time.time + 2.5f;
                turnRoutine = StartCoroutine(WaitAnim(2.5f));
                
                return false;
            }
            
            moveSpeed = patrolSpeed;
        }
        return moveSpeed > 0f;
    }

    public void FixedTick()
    {
        if(!enabled) return;
        if(Body == null) return;
        Body.velocity = Vector2.zero;
        float x = Mathf.MoveTowards(Body.position.x, moveTargetX, moveSpeed * Time.fixedDeltaTime);
        Body.MovePosition(new Vector2(x, Body.position.y));
    }

    public bool CanAttack()
    {
        if(!enabled || !foundPlayer ||player == null || !player.gameObject.activeInHierarchy 
            || player.CurrentHP <= 0 || Enemy == null || !Enemy.CanAct || Time.time < nextAttackTime) return false;
        float distance = Vector2.Distance(Body.position, player.transform.position);
        if(distance <= 3.5f && Time.time >= nextTackleTime)
        {
            AttackKind = EnemyAttackKind.Melee;
            return true;
        }
        if(distance > 8f || bulletPrefab == null) return false;
        AttackKind = EnemyAttackKind.FanShot;
        return true;
    }

    

    public IEnumerator Attack()
    {
        int version = ++attackVersion;
        attacking = true;
        Face(player.transform.position.x - Body.position.x);
        bool tackle = AttackKind == EnemyAttackKind.Melee;
        yield return Wait(tackle ? 0.4f : 0.35f, version);
        if(AttackIsActivate(version))
        {
            if(tackle){ 
            yield return Tackle(version);
            if(AttackIsActivate(version) && Enemy.Animation != null) Enemy.Animation.EndAttack(AttackKind);
            }
            else
            {
                Vector3 point = shotPoint != null ? shotPoint.position : transform.position;
                FanShotAttackPattern.Shoot(bulletPrefab, point,
                Vector2.right * facingX, 1, 0f, 7f, 4f, 15);
            }

            yield return Wait(tackle ? 0.5f : 0.65f, version);
        }
        if(version == attackVersion) Cancel();
        
    }

    IEnumerator Tackle(int version)
    {
        Vector2 direction = Vector2.right * facingX;
        float distance = 4.5f;
        bool hitPlayer = false;
        while(distance > 0f && AttackIsActivate(version))
        {
            float step = Mathf.Min(distance, Mathf.Max(0.1f, tackleSpeed) * Time.fixedDeltaTime);
            Bounds bounds = BodyCollider.bounds;
            Vector2 size = bounds.size;
            size.x += step;
            Vector2 center = (Vector2)bounds.center + direction * step * 0.5f;

            foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, size, 0f, playerLayers))
            {
                if(!hitPlayer && hit.GetComponentInParent<PlayerController>() == player)
                {
                    hitPlayer = true;
                    player.TakeDamage(new DamageRequest(tackleDamage, gameObject));
                }
                
            }
            if(!AttackIsActivate(version)) yield break;
            Body.MovePosition(Body.position + direction * step);
            distance -= step;
            yield return new WaitForFixedUpdate();
        }
        if(version == attackVersion && Enemy != null && !Enemy.IsDead) Body.velocity = Vector2.zero;
    }

    IEnumerator Wait(float seconds, int version)
    {
        while(seconds > 0f && AttackIsActivate(version))
        {
            seconds -= Time.deltaTime;
            yield return null;
        }
        
    }

    bool AttackIsActivate(int version)
    {
        return version == attackVersion && attacking && enabled && Enemy != null && Enemy.State == EnemyState.Attack
            && player != null && player.gameObject.activeInHierarchy && player.CurrentHP > 0;
        
    }

    public void Stop()
    {
        moveSpeed = 0f;
        if(turnRoutine != null)
        {
            StopCoroutine(turnRoutine);
            turnRoutine = null;
        }
        if(an != null)
        {
            an.SetBool("Search", false);
            an.SetBool("Damage", Enemy != null && Enemy.State == EnemyState.HitStun);
            an.ResetTrigger("Find");
        }
        if(Body != null && (Enemy == null || !Enemy.IsDead)) Body.velocity = Vector2.zero;
        if(attacking) Cancel();
        
    }

    public void Cancel()
    {
        if(attacking && AttackKind == EnemyAttackKind.Melee) nextTackleTime = Time.time + 4f;
        attacking = false;
        attackVersion++;
        nextAttackTime = Time.time + 2f;
        Stop();
        if(Enemy != null && Enemy.Animation != null) Enemy.Animation.EndAttack(AttackKind);
        
    }

    void Face(float direction)
    {
        if(Mathf.Abs(direction) < 0.01f) return;
        facingX = Mathf.Sign(direction);
        Vector3 scale = transform.localScale;
        scale.x = -facingX * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

}
