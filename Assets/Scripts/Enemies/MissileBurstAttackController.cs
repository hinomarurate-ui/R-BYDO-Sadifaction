using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class MissileBurstAttackController : StationaryMovement, IEnemyAttackPattern
{
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

    float nextAttackTime;

    public EnemyAttackKind AttackKind { get { return EnemyAttackKind.MissileBurst; } }

    protected override void OnInitialized()
    {
        if(audioSource == null)
        {
            audioSource = CachedAudioSource;
        }
    }

    public bool CanAttack()
    {
        if(Target == null || Time.time < nextAttackTime || Enemy == null || Enemy.IsDead)
        {
            return false;
        }

        return Vector2.Distance(transform.position, Target.position) < searchRange;
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
        if(bulletPrefab == null || Target == null)
        {
            yield break;
        }

        Vector3 origin = shotPoint != null ? shotPoint.position : transform.position;
        Vector2 baseDirection = (Target.position + Vector3.up * aimHeight - origin).normalized;
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

    void SetShotAnimation(bool enabled)
    {
        if(Enemy != null && Enemy.Animation != null)
        {
            Enemy.Animation.SetShot(enabled);
        }
    }

    void PlayShotSound()
    {
        if(audioSource != null && shotSound != null)
        {
            audioSource.PlayOneShot(shotSound, 1.25f);
        }
    }

}
