using System.Collections.Generic;
using UnityEngine;

public static class MeleeAttackPattern
{
    public static void Hit(Vector3 origin, float radius, LayerMask playerLayers, int damage)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius, playerLayers);
        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        foreach(Collider2D hit in hits)
        {
            if(hit == null) continue;

            IDamageable target = hit.GetComponentInParent<IDamageable>();
            if(target == null || damagedTargets.Contains(target)) continue;

            damagedTargets.Add(target);
            target.TakeDamage(new DamageRequest(damage, null, hit.bounds.center, 0f, 0f));
        }
    }
}

public static class FanShotAttackPattern
{
    public static void Shoot(
        GameObject bulletPrefab,
        Vector3 origin,
        Vector2 baseDirection,
        int bulletCount,
        float bulletAngleSpacing,
        float bulletSpeed,
        float bulletLifeTime,
        int damage)
    {
        if(bulletPrefab == null || baseDirection.sqrMagnitude <= 0f)
        {
            return;
        }

        for(int i = 0; i < bulletCount; i++)
        {
            float center = (bulletCount - 1) * 0.5f;
            float angle = (i - center) * bulletAngleSpacing;
            Vector2 direction = (Vector2)(Quaternion.Euler(0f, 0f, angle) * baseDirection.normalized);

            GameObject bullet = Object.Instantiate(bulletPrefab, origin, Quaternion.identity);
            EnemyBullet enemyBullet = bullet.GetComponent<EnemyBullet>();
            if(enemyBullet != null)
            {
                enemyBullet.Init(direction, bulletSpeed, bulletLifeTime, damage);
            }
        }
    }
}

public static class MissileBurstAttackPattern
{
    public static EnemyHomingMissile Spawn(
        GameObject bulletPrefab,
        Vector3 origin,
        Vector2 direction,
        float speed,
        float lifeTime,
        int damage)
    {
        if(bulletPrefab == null)
        {
            return null;
        }

        GameObject bullet = Object.Instantiate(bulletPrefab, origin, Quaternion.identity);
        EnemyHomingMissile enemyMissile = bullet.GetComponent<EnemyHomingMissile>();
        if(enemyMissile != null)
        {
            enemyMissile.Init(direction, speed, lifeTime, damage);
        }

        return enemyMissile;
    }
}

