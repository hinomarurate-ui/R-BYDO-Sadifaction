using System.Collections.Generic;
using UnityEngine;

// 実装意図: 近接判定を攻撃 controller から分離し、同じ円形ヒット処理を他の敵にも使い回せるようにする。
public static class MeleeAttackPattern
{
    public static void Hit(Vector3 origin, float radius, LayerMask playerLayers, int damage)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius, playerLayers);
        // 実装意図: 複数 collider を持つプレイヤーに同じ攻撃が多段ヒットしないようにする。
        HashSet<Tikuwa> damagedPlayers = new HashSet<Tikuwa>();

        foreach(Collider2D hit in hits)
        {
            if(hit == null) continue;

            Tikuwa player = hit.GetComponentInParent<Tikuwa>();
            if(player == null || damagedPlayers.Contains(player)) continue;

            damagedPlayers.Add(player);
            player.Damage(damage);
        }
    }
}

// 実装意図: 扇状射撃の弾生成を純粋な helper にして、攻撃タイミング管理から切り離す。
public static class FanShotAttackPattern
{
    public static void Shoot(
        GameObject bulletPrefab,
        Vector3 origin,
        Vector2 baseDirection,
        int bulletCount,
        float bulletAngleSpace,
        float bulletSpeed,
        float bulletLifeTime)
    {
        if(bulletPrefab == null || baseDirection.sqrMagnitude <= 0f)
        {
            return;
        }

        for(int i = 0; i < bulletCount; i++)
        {
            float center = (bulletCount - 1) * 0.5f;
            float angle = (i - center) * bulletAngleSpace;
            Vector2 direction = (Vector2)(Quaternion.Euler(0f, 0f, angle) * baseDirection.normalized);

            GameObject bullet = Object.Instantiate(bulletPrefab, origin, Quaternion.identity);
            EnemyBullet enemyBullet = bullet.GetComponent<EnemyBullet>();
            if(enemyBullet != null)
            {
                enemyBullet.Init(direction, bulletSpeed, bulletLifeTime);
            }
        }
    }
}

// 実装意図: Pstaff のミサイル生成を helper 化し、将来のミサイル連射敵でも同じ弾初期化を使う。
public static class MissileBurstAttackPattern
{
    public static EnemyMissile Spawn(
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
        EnemyMissile enemyMissile = bullet.GetComponent<EnemyMissile>();
        if(enemyMissile != null)
        {
            enemyMissile.Init(direction, speed, lifeTime, damage);
        }

        return enemyMissile;
    }
}
