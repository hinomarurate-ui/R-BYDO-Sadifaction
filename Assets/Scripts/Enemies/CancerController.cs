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
        bool visible = player != null && player.gameObject.activateInHierarchy && player.CurrentHP > 0
            && Vector2.Distance(body.position, player.transform.position) <= range;
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

    public bool Tick()
    {
        moveSpeed = 0f;
        if(!enabled || Time.time < waitTime) return false;
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
                Face(-facingX);
                waitTime = Time.time + 0.4f;
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

    public void CanAttack()
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
                point.x = Mathf.Abs(point.x) * facingX;
                FanShotAttackPattern.Shoot(bulletPrefab, transform.TransformPoint(point),
                    Vector2.right * facingX, 1, 0f, 7f, 4f, 15);
            }
            yield return Wait(tackle ? 0.5f : 0.45f, version);
        }
        if(version == attackVersion) Cancel();
        
    }

    IEnumerator Tackle(int version)
    {
        
    }

    IEnumerator Wait(float seconds, int version)
    {
        
    }

    bool AttackIsActivate(int version)
    {
        
    }

    public void Stop()
    {
        
    }

    public void Cancel()
    {
        
    }

    void Face(float direction)
    {
        
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
