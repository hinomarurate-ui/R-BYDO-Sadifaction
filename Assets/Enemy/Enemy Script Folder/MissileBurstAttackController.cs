using System.Collections;
using UnityEngine;

// 実装意図: Pstaff の静止 + ミサイル連射を 1 component で扱い、既存 scene override の field 名を残す。
public class MissileBurstAttackController : StationaryMovement, IEnemyAttackPattern
{
    // 実装意図: 移行済み Pstaff は旧 prefab/scene の値を優先し、新規敵だけ Definition 主導にできる。
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
        // 実装意図: 攻撃 coroutine 中の再突入を防ぎ、撃ち終わってから cooldown を数える旧挙動を維持する。
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
        // 実装意図: 被弾などで中断されても、即再攻撃せず cooldown を挟ませる。
        if(enemy != null && enemy.Animation != null)
        {
            enemy.Animation.SetShot(false);
        }

        nextAttackTime = Time.time + attackCooltime;
    }

    IEnumerator ShotBurst()
    {
        // 実装意図: 1 発ずつ間隔を空けて撃ち、動画/既存 Pstaff の連射感を維持する。
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
        // 実装意図: 新規ミサイル敵では asset 側の AttackSettings だけでテンポと弾数を調整する。
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
