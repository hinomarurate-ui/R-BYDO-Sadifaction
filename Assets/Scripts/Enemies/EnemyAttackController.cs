using System.Collections;
using UnityEngine;

public class EnemyAttackController : MonoBehaviour, IEnemyAttackPattern
{
    [SerializeField] protected bool useDefinitionSettings = true;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform shotPoint;
    [SerializeField] protected MonoBehaviour enemyMove;
    [SerializeField] protected EnemyController enemy;
    [SerializeField] protected float searchRange = 8f;
    [SerializeField] protected float attackCooltime = 2f;
    [SerializeField] protected float shotChargetime = 0.35f;
    [SerializeField] protected float attackEndTime = 0.25f;
    [SerializeField] protected float bulletSpeed = 6f;
    [SerializeField] protected float bulletLifeTime = 3f;
    [SerializeField] protected int bulletDamage = 15;
    [SerializeField] protected int bulletCount = 1;
    [SerializeField] protected float bulletAngleSpace = 12f;
    [SerializeField] protected float aimHeight = 0.5f;

    [Header("Melee Attack")]
    [SerializeField] protected bool useMeleeAttack;
    [SerializeField] protected float meleeChargetime = 0.35f;
    [SerializeField] protected float meleeSearchRange = 1.6f;
    [SerializeField] protected float meleeRadius = 0.8f;
    [SerializeField] protected int meleeDamage = 15;
    [SerializeField] protected LayerMask playerLayers;
    [SerializeField] protected Transform meleePoint;

    protected Transform player;
    protected Rigidbody2D rb;
    protected float lastAttackTime = -999f;
    protected EnemyAttackKind selectedAttackKind = EnemyAttackKind.None;

    public EnemyAttackKind AttackKind { get { return selectedAttackKind; } }

    public virtual void Initialize(EnemyController controller)
    {
        enemy = controller;
        rb = GetComponent<Rigidbody2D>();
        enemyMove = enemyMove != null ? enemyMove : GetComponent<GroundPatrolMovement>();
        player = controller != null ? controller.Target : null;
        if(useDefinitionSettings)
        {
            ApplyDefinition(controller != null ? controller.Definition : null);
        }
    }

    public virtual bool CanAttack()
    {
        if(player == null || enemy == null || !enemy.IsGrounded || Time.time < lastAttackTime + attackCooltime)
        {
            return false;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        if(useMeleeAttack && distance <= meleeSearchRange)
        {
            selectedAttackKind = EnemyAttackKind.Melee;
            return true;
        }

        if(distance <= searchRange)
        {
            selectedAttackKind = EnemyAttackKind.FanShot;
            return true;
        }

        selectedAttackKind = EnemyAttackKind.None;
        return false;
    }

    public virtual IEnumerator Attack()
    {
        lastAttackTime = Time.time;
        StopBodyVelocity();

        if(selectedAttackKind == EnemyAttackKind.Melee)
        {
            yield return new WaitForSeconds(meleeChargetime);
            DoMeleeAttack();
        }
        else if(selectedAttackKind == EnemyAttackKind.FanShot)
        {
            yield return new WaitForSeconds(shotChargetime);
            ShotFan();
        }

        if(attackEndTime > 0f)
        {
            yield return new WaitForSeconds(attackEndTime);
        }
    }

    public virtual void Cancel()
    {
        selectedAttackKind = EnemyAttackKind.None;
    }

    protected virtual void ShotFan()
    {
        if(player == null) return;

        Vector3 origin = shotPoint != null ? shotPoint.position : transform.position;
        Vector2 targetPosition = player.position + Vector3.up * aimHeight;
        Vector2 baseDirection = (targetPosition - (Vector2)origin).normalized;
        FanShotAttackPattern.Shoot(bulletPrefab, origin, baseDirection, bulletCount, bulletAngleSpace, bulletSpeed, bulletLifeTime, bulletDamage);
    }

    protected virtual void DoMeleeAttack()
    {
        Vector3 origin = meleePoint != null ? meleePoint.position : transform.position;
        MeleeAttackPattern.Hit(origin, meleeRadius, playerLayers, meleeDamage);
    }

    protected virtual void ApplyDefinition(EnemyDefinition definition)
    {
        if(definition == null) return;

        EnemyDefinition.AttackSettings settings = definition.Attack;
        if(settings == null) return;

        searchRange = settings.searchRange;
        attackCooltime = settings.attackCooltime;
        shotChargetime = settings.chargeTime;
        attackEndTime = settings.endTime;
        bulletSpeed = settings.bulletSpeed;
        bulletLifeTime = settings.bulletLifeTime;
        bulletDamage = settings.bulletDamage;
        bulletCount = settings.bulletCount;
        bulletAngleSpace = settings.bulletAngleSpace;
        aimHeight = settings.aimHeight;
        useMeleeAttack = settings.useMeleeAttack;
        meleeChargetime = settings.meleeChargeTime;
        meleeSearchRange = settings.meleeSearchRange;
        meleeRadius = settings.meleeRadius;
        meleeDamage = settings.meleeDamage;

        if(playerLayers.value == 0)
        {
            playerLayers = settings.playerLayers;
        }
    }

    protected void StopBodyVelocity()
    {
        if(rb != null)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }
}
