using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    [Header("Core")]
    [SerializeField] int maxHealth = 5;
    [SerializeField] int score = 200;
    [SerializeField] float hitStunTime = 0.12f;

    [Header("Death")]
    [SerializeField] DeathSettings death = new DeathSettings();

    [Header("Animation")]
    [SerializeField] AnimationSettings animation = new AnimationSettings();

    public virtual int MaxHealth { get { return maxHealth; } }
    public virtual int Score { get { return score; } }
    public virtual float HitStunTime { get { return hitStunTime; } }
    public virtual DeathSettings Death { get { return death; } }
    public virtual AnimationSettings Animation { get { return animation; } }

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
