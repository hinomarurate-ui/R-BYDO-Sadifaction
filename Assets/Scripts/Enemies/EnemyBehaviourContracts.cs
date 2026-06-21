using System.Collections;

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
