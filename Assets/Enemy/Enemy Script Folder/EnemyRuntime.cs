using System.Collections;
using UnityEngine;

// 実装意図: すべての敵が共有する最小の状態だけをここに集約し、個別敵の分岐を Controller 側へ散らさない。
public enum EnemyState
{
    Idle,
    Move,
    Attack,
    HitStun,
    Dead
}

// 実装意図: Animator の既存パラメータへ攻撃演出をつなぐため、攻撃パターンの種類を共通名で扱う。
public enum EnemyAttackKind
{
    None,
    Melee,
    FanShot,
    MissileBurst
}

// 実装意図: プレイヤー弾・近接・ミサイルが敵HPの具象型に依存せず、ダメージ対象を共通処理できるようにする。
public interface IDamageable
{
    bool TakeDamage(int amount);
}

// 実装意図: 地上巡回・静止・将来の空中移動を EnemyController から同じ手順で呼び出せるようにする。
public interface IEnemyMovement
{
    void Initialize(EnemyController controller);
    bool Tick();
    void FixedTick();
    void Stop();
}

// 実装意図: 近接・扇状射撃・ミサイル連射を差し替え式にして、敵追加時に Controller を増やさない。
public interface IEnemyAttackPattern
{
    EnemyAttackKind AttackKind { get; }
    void Initialize(EnemyController controller);
    bool CanAttack();
    IEnumerator Attack();
    void Cancel();
}

// 実装意図: 動画のような敵固有ルーチンを後から追加するための拡張口として空けておく。
public interface IEnemyRoutine
{
    void Initialize(EnemyController controller);
    void Tick();
}
