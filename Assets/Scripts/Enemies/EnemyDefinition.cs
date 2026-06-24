using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Game/Enemy Definition")]
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
    public class GroundPatrolSettings
    {
        public float moveSpeed = 2f;
        public float directionX = -1f;
        public float jumpImpulse = 5f;
        public float jumpPower = 0.6f;
        [FormerlySerializedAs("jumpSlow")]
        public float jumpSlowHeight = 2f;
        [FormerlySerializedAs("maxHeight")]
        public float maxJumpHeight = 0.7f;
        public float jumpChargeTime = 0.5f;
        public float postJumpLockTime = 0.3f;
    }

    [Serializable]
    public class AttackSettings
    {
        public float searchRange = 8f;
        [FormerlySerializedAs("attackCooltime")]
        public float attackCooldown = 2f;
        public float chargeTime = 0.35f;
        public float endTime = 0.25f;
        public float bulletSpeed = 6f;
        public float bulletLifeTime = 3f;
        public int bulletCount = 1;
        [FormerlySerializedAs("bulletAngleSpace")]
        public float bulletAngleSpacing = 12f;
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
