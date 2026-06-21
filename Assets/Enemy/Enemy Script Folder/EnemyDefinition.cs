using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy Definition")]
// 実装意図: HP・スコア・移動・攻撃・死亡・Animator 名を prefab ではなくデータとして共有できるようにする。
public class EnemyDefinition : ScriptableObject
{
    [Header("Core")]
    [SerializeField] int maxHealth = 5;
    [SerializeField] int score = 200;
    [SerializeField] float hitStunTime = 0.12f;

    [Header("Movement")]
    [SerializeField] GroundPatrolSettings groundPatrol = new GroundPatrolSettings();

    [Header("Attack")]
    [SerializeField] AttackSettings attack = new AttackSettings();

    [Header("Death")]
    [SerializeField] DeathSettings death = new DeathSettings();

    [Header("Animation")]
    [SerializeField] AnimationSettings animation = new AnimationSettings();

    public virtual int MaxHealth { get { return maxHealth; } }
    public virtual int Score { get { return score; } }
    public virtual float HitStunTime { get { return hitStunTime; } }
    public virtual GroundPatrolSettings GroundPatrol { get { return groundPatrol; } }
    public virtual AttackSettings Attack { get { return attack; } }
    public virtual DeathSettings Death { get { return death; } }
    public virtual AnimationSettings Animation { get { return animation; } }

    [Serializable]
    // 実装意図: 地上巡回のチューニング値を敵種ごとに持たせ、移動コンポーネントの差し替えに備える。
    public class GroundPatrolSettings
    {
        public float moveSpeed = 2f;
        public float directionX = -1f;
        public float jumpImpulse = 5f;
        public float jumpPower = 0.6f;
        public float jumpSlow = 2f;
        public float maxHeight = 0.7f;
        public float jumpChargeTime = 0.5f;
        public float postJumpLockTime = 0.3f;
    }

    [Serializable]
    // 実装意図: 攻撃テンポや弾性能を攻撃パターン実装から分離し、バランス調整を asset 側に寄せる。
    public class AttackSettings
    {
        public float searchRange = 8f;
        public float attackCooltime = 2f;
        public float chargeTime = 0.35f;
        public float endTime = 0.25f;
        public float bulletSpeed = 6f;
        public float bulletLifeTime = 3f;
        public int bulletCount = 1;
        public float bulletAngleSpace = 12f;
        public float bulletInterval = 0.1f;
        public int bulletDamage = 15;
        public float aimHeight = 0.5f;
        public bool useMeleeAttack = true;
        public float meleeChargeTime = 0.35f;
        public float meleeSearchRange = 5f;
        public float meleeRadius = 2f;
        public int meleeDamage = 15;
        public LayerMask playerLayers;
    }

    [Serializable]
    // 実装意図: 撃破時のスコア・SE・爆発・吹き飛びを共通死亡処理から参照できるようにまとめる。
    public class DeathSettings
    {
        public float deathTorque = 5f;
        public float fadeTime = 1f;
        public float smashX = 8f;
        public float smashY = 5f;
        public GameObject bombPrefab;
        public float bombLifeTime = 1f;
        public AudioClip damageSound;
        public AudioClip deathSound;
    }

    [Serializable]
    // 実装意図: 既存 Animator Controller を維持したまま、コード側で使うパラメータ名を敵ごとに差し替える。
    public class AnimationSettings
    {
        public string walkBool = "Walk";
        public string jumpBool = "Jump";
        public string jumpChargeBool = "JumpC";
        public string shotBool = "Shot";
        public string attackBool = "Attack";
        public string missileBool = "Missile";
        public string deathTrigger = "Death";
        public string missileFinishTrigger = "MissileFinish";
    }
}
