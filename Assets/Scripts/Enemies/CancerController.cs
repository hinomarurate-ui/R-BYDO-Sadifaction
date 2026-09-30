using System.Collections;
using UnityEngine;

public class CancerController : MonoBehaviour, IEnemyMovement, IEnemyAttackPattern,IEnemyRoutine
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

    EnemyController enemy;
    Rigidbody2D body;
    Collider2D bodyCollider;
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
    bool Initialized;
    int attackVersion;

    public EnemyAttackKind AttackKind { get; private set;}

    // Update is called once per frame
    public void Initialize(EnemyController controller)
    {
        enemy = controller;
        if(Initialized) return;
        Initialized = true;
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        body.gravityScale = 0f;
        patrolCenter = body.position;
        moveTargetX = body.position.x;
        foundPlayer = false;
        waitTime = Time.time;
        Face(-1f);
        
    }

    void IEnemyRoutine.Tick()
    {
        if(!enabled) return;
        if(player == null)
        {
            GameObject target = GameObject.FindWithTag("Player");
            if(target != null) player = target.GetComponent<PlayerController>();
        }
        float range = foundPlayer ? searchRange + 4f : searchRange;
        bool visible = player != null && player.gameObject.activeInHierarchy && player.CurrentHP > 0
            && Vector2.Distance(body.position, player.transform.position) <= range;
            visible = visible && (player.transform.position.x - body.position.x) * facingX > 0f;
        if(visible == foundPlayer) return;

        foundPlayer = visible;
        Stop();
        waitTime = Time.time + 0.55f;
        if(foundPlayer)
        {
            Face(player.transform.position.x - body.position.x);
            nextAttackTime = waitTime + 2f;
        }
        else patrolCenter = body.position;
    }

    IEnumerator WaitAnim(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Debug.Log("ちくわ");
    }

    public bool Tick()
    {
        moveSpeed = 0f;
        if(!enabled || Time.time < waitTime) return false;
        an.SetBool("Search", false);
        if(foundPlayer)
        {
            float difference = player.transform.position.x - body.position.x;
            Face(difference);
            if(Mathf.Abs(difference) <= 0.75f) return false;
            moveTargetX = player.transform.position.x - facingX * 0.75f;
            moveSpeed = chaseSpeed;
        }
        else
        {
            moveTargetX = patrolCenter.x + facingX * patrolHalfWidth;
            if(Mathf.Abs(moveTargetX - body.position.x) < 0.02f)
            {
                an.SetBool("Search", true);
                waitTime = Time.time + 2.5f;
                Face(-facingX);
                
                return false;
            }
            
            moveSpeed = patrolSpeed;
        }
        return moveSpeed > 0f;
    }

    public void FixedTick()
    {
        if(!enabled) return;
        body.velocity = Vector2.zero;
        float x = Mathf.MoveTowards(body.position.x, moveTargetX, moveSpeed * Time.fixedDeltaTime);
        body.MovePosition(new Vector2(x, body.position.y));
    }

    public bool CanAttack()
    {
        if(!enabled || !foundPlayer || !enemy.CanAct || Time.time < nextAttackTime) return false;
        float distance = Vector2.Distance(body.position, player.transform.position);
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
        Face(player.transform.position.x - body.position.x);
        bool tackle = AttackKind == EnemyAttackKind.Melee;
        yield return Wait(tackle ? 0.4f : 0.35f, version);
        if(AttackIsActivate(version))
        {
            if(tackle) yield return Tackle(version);
            else
            {
                Vector3 point = shotPoint.localPosition;
                point.x = -Mathf.Abs(point.x);
                FanShotAttackPattern.Shoot(bulletPrefab, transform.TransformPoint(point),
                Vector2.right * facingX, 1, 0f, 7f, 4f, 15);
            }
            yield return Wait(tackle ? 0.5f : 0.45f, version);
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
            Bounds bounds = bodyCollider.bounds;
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
            body.MovePosition(body.position + direction * step);
            distance -= step;
            yield return new WaitForFixedUpdate();
        }
        if(version == attackVersion && !enemy.IsDead) body.velocity = Vector2.zero;
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
        return version == attackVersion && attacking && enabled && enemy.State == EnemyState.Attack
            && player != null && player.gameObject.activeInHierarchy && player.CurrentHP > 0;
        
    }

    public void Stop()
    {
        moveSpeed = 0f;
        if(body != null && !enemy.IsDead) body.velocity = Vector2.zero;
        if(attacking) Cancel();
        
    }

    public void Cancel()
    {
        if(attacking && AttackKind == EnemyAttackKind.Melee) nextTackleTime = Time.time + 4f;
        attacking = false;
        attackVersion++;
        nextAttackTime = Time.time + 2f;
        Stop();
        enemy.Animation.EndAttack(AttackKind);
        
    }

    void Face(float direction)
    {
        StartCoroutine(WaitAnim(2.5f));
        if(Mathf.Abs(direction) < 0.01f) return;
        facingX = Mathf.Sign(direction);
        Vector3 scale = transform.localScale;
        scale.x = -facingX * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void OnEnable()
    {
        if(enemy != null && !enemy.IsDead) Initialize(enemy);
    }

    void OnDisable()
    {
        Stop();
        Initialized = false;
    }


}
