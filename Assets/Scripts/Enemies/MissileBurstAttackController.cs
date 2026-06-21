using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class MissileBurstAttackController : StationaryMovement, IEnemyAttackPattern
{
    [SerializeField] bool useDefinitionSettings = true;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform shotPoint;
    [SerializeField] float searchRange = 8f;
    [FormerlySerializedAs("attackCooltime")]
    [SerializeField] float attackCooldown = 2f;
    [FormerlySerializedAs("shotChargetime")]
    [SerializeField] float shotChargeTime = 0.35f;
    [SerializeField] float attackEndTime = 0.25f;
    [SerializeField] float bulletSpeed = 6f;
    [SerializeField] float bulletLifeTime = 3f;
    [SerializeField] int bulletCount = 1;
    [FormerlySerializedAs("bulletAngleSpace")]
    [SerializeField] float bulletAngleSpacing = 12f;
    [SerializeField] float bulletInterval = 0.1f;
    [SerializeField] int bulletDamage = 15;
    [SerializeField] float aimHeight = 0.5f;

    [FormerlySerializedAs("Mis")]
    [SerializeField] AudioClip shotSound;
    [FormerlySerializedAs("As")]
    [SerializeField] AudioSource audioSource;

    Transform player;
    float nextAttackTime;

    public EnemyAttackKind AttackKind { get { return EnemyAttackKind.MissileBurst; } }

    public override void Initialize(EnemyController controller)
    {
        base.Initialize(controller);
        player = controller != null ? controller.Target : null;

        if(audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
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

        yield return WaitIfPositive(shotChargeTime);
        SetShotAnimation(true);
        yield return StartCoroutine(ShotBurst());
        yield return WaitIfPositive(attackEndTime);
        SetShotAnimation(false);

        nextAttackTime = Time.time + attackCooldown;
    }

    public void Cancel()
    {
        SetShotAnimation(false);
        nextAttackTime = Time.time + attackCooldown;
    }

    IEnumerator ShotBurst()
    {
        if(bulletPrefab == null || player == null)
        {
            yield break;
        }

        Vector3 origin = shotPoint != null ? shotPoint.position : transform.position;
        Vector2 baseDirection = (player.position + Vector3.up * aimHeight - origin).normalized;
        int shots = Mathf.Max(0, bulletCount);

        for(int i = 0; i < shots; i++)
        {
            float center = (shots - 1) * 0.5f;
            float angle = (i - center) * bulletAngleSpacing;
            Vector2 direction = (Vector2)(Quaternion.Euler(0f, 0f, angle) * baseDirection);

            MissileBurstAttackPattern.Spawn(bulletPrefab, origin, direction, bulletSpeed, bulletLifeTime, bulletDamage);
            PlayShotSound();

            if(i < shots - 1)
            {
                yield return WaitIfPositive(bulletInterval);
            }
        }
    }

    void ApplyDefinition(EnemyDefinition definition)
    {
        if(definition == null || definition.Attack == null)
        {
            return;
        }

        EnemyDefinition.AttackSettings settings = definition.Attack;
        searchRange = settings.searchRange;
        attackCooldown = settings.attackCooldown;
        shotChargeTime = settings.chargeTime;
        attackEndTime = settings.endTime;
        bulletSpeed = settings.bulletSpeed;
        bulletLifeTime = settings.bulletLifeTime;
        bulletCount = settings.bulletCount;
        bulletAngleSpacing = settings.bulletAngleSpacing;
        bulletInterval = settings.bulletInterval;
        bulletDamage = settings.bulletDamage;
        aimHeight = settings.aimHeight;
    }

    void SetShotAnimation(bool enabled)
    {
        if(enemy != null && enemy.Animation != null)
        {
            enemy.Animation.SetShot(enabled);
        }
    }

    void PlayShotSound()
    {
        if(audioSource != null && shotSound != null)
        {
            audioSource.PlayOneShot(shotSound, 1.25f);
        }
    }

    static IEnumerator WaitIfPositive(float duration)
    {
        if(duration > 0f)
        {
            yield return new WaitForSeconds(duration);
        }
    }
}
