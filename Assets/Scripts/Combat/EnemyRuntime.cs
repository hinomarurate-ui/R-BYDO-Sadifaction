using System.Collections;
using UnityEngine;

public struct DamageRequest
{
    public readonly int Amount;
    public readonly GameObject Source;
    public readonly Vector2 HitPoint;
    public readonly float KillShakePower;
    public readonly float KillShakeTime;

    public DamageRequest(int amount, GameObject source, Vector2 hitPoint, float killShakePower, float killShakeTime)
    {
        Amount = amount;
        Source = source;
        HitPoint = hitPoint;
        KillShakePower = killShakePower;
        KillShakeTime = killShakeTime;
    }

    public DamageRequest(int amount, GameObject source)
        : this(amount, source, Vector2.zero, 0f, 0f)
    {
    }
}

public struct DamageResult
{
    public readonly bool Applied;
    public readonly bool Killed;
    public readonly int CurrentHealth;
    public readonly int MaxHealth;

    public DamageResult(bool applied, bool killed, int currentHealth, int maxHealth)
    {
        Applied = applied;
        Killed = killed;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
    }

    public static DamageResult Ignored(int currentHealth, int maxHealth)
    {
        return new DamageResult(false, false, currentHealth, maxHealth);
    }
}

public interface IDamageable
{
    DamageResult TakeDamage(DamageRequest request);
}

public enum EnemyState
{
    Idle,
    Move,
    Attack,
    HitStun,
    Dead
}

public enum EnemyAttackKind
{
    None,
    Melee,
    FanShot,
    MissileBurst
}

public interface IEnemyMovement
{
    void Initialize(EnemyController controller);
    bool Tick();
    void FixedTick();
    void Stop();
}

public interface IEnemyAttackPattern
{
    EnemyAttackKind AttackKind { get; }
    void Initialize(EnemyController controller);
    bool CanAttack();
    IEnumerator Attack();
    void Cancel();
}

public interface IEnemyRoutine
{
    void Initialize(EnemyController controller);
    void Tick();
}
