using System.Collections;
using UnityEngine;

public class MissileBurstAttackController : StationaryMovement, IEnemyAttackPattern
{
    [SerializeField] bool useDefinitionSettings = true;

    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform shotPoint;
    [SerializeField] float searchRange = 8f;
    [SerializeField] float attackCooltime = 2f;
    [SerializeField] float shotChargetime = 0.35f;
    [SerializeField] float attackEndTime = 0.25f;
    [SerializeField] float bulletSpeed = 6f;
    [SerializeField] float bulletLifeTime = 3f;
    [SerializeField] int bulletCount = 1;
    [SerializeField] float bulletAngleSpace = 12f;
    [SerializeField] float bulletInterval = 0.1f;
    [SerializeField] int bulletDamage = 15;
    [SerializeField] float aimHeight = 0.5f;

    [SerializeField] AudioClip Mis;
    [SerializeField] AudioSource As;

    Transform player;
    float nextAttackTime;

    public EnemyAttackKind AttackKind { get { return EnemyAttackKind.MissileBurst; } }

    public override void Initialize(EnemyController controller)
    {
        base.Initialize(controller);
        player = controller != null ? controller.Target : null;

        if(As == null)
        {
            As = GetComponent<AudioSource>();
        }

        if(useDefinitionSettings)
        {
            ApplyDefinition(controller != null ? controller.Definition : null);
        }
    }

    public bool CanAttack()
    {
        if(player == null || Time.time < nextAttackTime || enemy == null || enemy.IsDead)
        {
            return false;
        }

        return Vector2.Distance(transform.position, player.position) < searchRange;
    }

    public IEnumerator Attack()
    {
        nextAttackTime = float.PositiveInfinity;
        Stop();

        if(shotChargetime > 0f)
        {
            yield return new WaitForSeconds(shotChargetime);
        }

        if(enemy != null && enemy.Animation != null)
        {
            enemy.Animation.SetShot(true);
        }

        yield return StartCoroutine(ShotBurst());

        if(attackEndTime > 0f)
        {
            yield return new WaitForSeconds(attackEndTime);
        }

        if(enemy != null && enemy.Animation != null)
        {
            enemy.Animation.SetShot(false);
        }

        nextAttackTime = Time.time + attackCooltime;
    }

    public void Cancel()
    {
        if(enemy != null && enemy.Animation != null)
        {
            enemy.Animation.SetShot(false);
        }

        nextAttackTime = Time.time + attackCooltime;
    }

    IEnumerator ShotBurst()
    {
        if(bulletPrefab == null || player == null)
        {
            yield break;
        }

        Vector3 origin = shotPoint != null ? shotPoint.position : transform.position;
        Vector2 baseDirection = (player.position + Vector3.up * aimHeight - origin).normalized;

        for(int i = 0; i < bulletCount; i++)
        {
            float center = (bulletCount - 1) * 0.5f;
            float angle = (i - center) * bulletAngleSpace;
            Vector2 direction = (Vector2)(Quaternion.Euler(0f, 0f, angle) * baseDirection);

            MissileBurstAttackPattern.Spawn(bulletPrefab, origin, direction, bulletSpeed, bulletLifeTime, bulletDamage);
            PlayShotSound();

            if(i < bulletCount - 1 && bulletInterval > 0f)
            {
                yield return new WaitForSeconds(bulletInterval);
            }
        }
    }

    void ApplyDefinition(EnemyDefinition definition)
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
        bulletCount = settings.bulletCount;
        bulletAngleSpace = settings.bulletAngleSpace;
        bulletInterval = settings.bulletInterval;
        bulletDamage = settings.bulletDamage;
        aimHeight = settings.aimHeight;
    }

    void PlayShotSound()
    {
        if(As != null && Mis != null)
        {
            As.PlayOneShot(Mis, 1.25f);
        }
    }
}
