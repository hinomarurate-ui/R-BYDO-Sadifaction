using System.Collections;

public interface IEnemyBehaviour
{
    void Initialize(EnemyController controller);
}

public interface IEnemyMovement : IEnemyBehaviour
{
    bool Tick();
    void FixedTick();
    void Stop();
}

public interface IEnemyAttackPattern : IEnemyBehaviour
{
    EnemyAttackKind AttackKind { get; }
    bool CanAttack();
    IEnumerator Attack();
    void Cancel();
}

public interface IEnemyRoutine : IEnemyBehaviour
{
    void Tick();
}
