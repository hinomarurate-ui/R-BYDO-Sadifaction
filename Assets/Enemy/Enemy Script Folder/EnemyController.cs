using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 実装意図: 敵の状態選択だけを担当し、移動・攻撃・HP・固有ルーチンは interface 経由で差し替える中核。
public class EnemyController : MonoBehaviour
{
    [SerializeField] EnemyDefinition definition;

    // 実装意図: 旧 prefab/scene の serialized field 名を残して、移行中の参照切れと override 破壊を避ける。
    [Header("Legacy Serialized References")]
    [SerializeField] protected GroundCheck ground;
    [SerializeField] protected GakeChecker gake;
    [SerializeField] protected EnemyDefinition Status;
    [SerializeField] protected EnemyHealth HP;
    [SerializeField] protected MonoBehaviour Move;

    public bool isGround = false;
    public bool isGake = false;

    readonly EnemySensor sensor = new EnemySensor();
    readonly List<IEnemyAttackPattern> attackPatterns = new List<IEnemyAttackPattern>();
    readonly List<IEnemyRoutine> routines = new List<IEnemyRoutine>();

    IEnemyMovement movement;
    Coroutine runningAttack;
    Coroutine hitStunRoutine;
    EnemyAnimationDriver animationDriver;
    bool initialized;

    public EnemyDefinition Definition { get; private set; }
    public EnemyState State { get; private set; } = EnemyState.Idle;
    public Transform Target { get; private set; }
    public EnemyAnimationDriver Animation { get { return animationDriver; } }
    public bool IsGrounded { get { return sensor.IsGrounded; } }
    public bool IsAtLedge { get { return sensor.IsAtLedge; } }
    public bool IsDead { get { return State == EnemyState.Dead; } }
    public bool CanAct { get { return State != EnemyState.Dead && State != EnemyState.HitStun && State != EnemyState.Attack; } }

    protected virtual void Awake()
    {
        InitializeRuntime();
    }

    protected virtual void OnEnable()
    {
        InitializeRuntime();
    }

    void InitializeRuntime()
    {
        // 実装意図: EnemyActivator で enable/disable されても初期化を二重に走らせない。
        if(initialized)
        {
            return;
        }

        initialized = true;
        Definition = definition != null ? definition : Status;
        sensor.Initialize(transform, ground, gake);
        animationDriver = new EnemyAnimationDriver(GetComponent<Animator>(), Definition);

        ResolveTarget();
        ResolveHealth();
        ResolveMovement();
        ResolveAttackPatterns();
        ResolveRoutines();
        RefreshSensorState();
    }

    protected virtual void Update()
    {
        RefreshSensorState();

        // 実装意図: Dead/HitStun は通常行動へ戻さず、移動だけ止めて割り込み状態を守る。
        if(State == EnemyState.Dead || State == EnemyState.HitStun)
        {
            StopMovement();
            return;
        }

        TickRoutines();

        if(State != EnemyState.Attack && TryStartAttack())
        {
            return;
        }

        if(State != EnemyState.Attack)
        {
            TickMovement();
        }
    }

    protected virtual void FixedUpdate()
    {
        RefreshSensorState();

        if(CanAct && movement != null)
        {
            movement.FixedTick();
        }
    }

    public void ChangeState(EnemyState nextState)
    {
        // 実装意図: Dead は最優先の終端状態として扱い、他状態へ戻らないようにする。
        if(State == EnemyState.Dead && nextState != EnemyState.Dead)
        {
            return;
        }

        if(State == nextState)
        {
            return;
        }

        State = nextState;
        animationDriver.ApplyState(State);
    }

    public void NotifyDamaged(bool killed)
    {
        if(killed)
        {
            EnterDead();
            return;
        }

        EnterHitStun();
    }

    public void EnterDead()
    {
        if(State == EnemyState.Dead) return;

        CancelAttack();
        StopMovement();
        ChangeState(EnemyState.Dead);
    }

    public void ForceStop()
    {
        CancelAttack();
        StopMovement();
        ChangeState(EnemyState.Idle);
    }

    void ResolveTarget()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        Target = playerObject != null ? playerObject.transform : null;
    }

    void ResolveHealth()
    {
        if(HP == null)
        {
            HP = GetComponent<EnemyHealth>();
        }

        if(HP != null)
        {
            HP.Initialize(this);
        }
    }

    void ResolveMovement()
    {
        // 実装意図: 旧 Move 参照があれば優先し、未設定 prefab でも同一 GameObject 上の移動 component を拾う。
        if(Move != null && Move is IEnemyMovement)
        {
            movement = (IEnemyMovement)Move;
        }
        else
        {
            foreach(MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
            {
                if(behaviour == this) continue;

                IEnemyMovement candidate = behaviour as IEnemyMovement;
                if(candidate != null)
                {
                    movement = candidate;
                    break;
                }
            }
        }

        if(movement != null)
        {
            movement.Initialize(this);
        }
    }

    void ResolveAttackPatterns()
    {
        attackPatterns.Clear();

        // 実装意図: 複数の攻撃 component を同居可能にし、CanAttack が true のものを優先順で使う。
        foreach(MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
        {
            IEnemyAttackPattern attackPattern = behaviour as IEnemyAttackPattern;
            if(attackPattern == null) continue;

            attackPattern.Initialize(this);
            attackPatterns.Add(attackPattern);
        }
    }

    void ResolveRoutines()
    {
        routines.Clear();

        // 実装意図: 特殊行動を追加しても Controller 本体を編集しないため、IEnemyRoutine を自動収集する。
        foreach(MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
        {
            IEnemyRoutine routine = behaviour as IEnemyRoutine;
            if(routine == null) continue;

            routine.Initialize(this);
            routines.Add(routine);
        }
    }

    void RefreshSensorState()
    {
        sensor.Refresh();
        isGround = sensor.IsGrounded;
        isGake = sensor.IsAtLedge;
    }

    void TickRoutines()
    {
        for(int i = 0; i < routines.Count; i++)
        {
            routines[i].Tick();
        }
    }

    void TickMovement()
    {
        bool moved = movement != null && movement.Tick();
        ChangeState(moved ? EnemyState.Move : EnemyState.Idle);
    }

    bool TryStartAttack()
    {
        for(int i = 0; i < attackPatterns.Count; i++)
        {
            if(attackPatterns[i].CanAttack())
            {
                runningAttack = StartCoroutine(RunAttack(attackPatterns[i]));
                return true;
            }
        }

        return false;
    }

    IEnumerator RunAttack(IEnemyAttackPattern attackPattern)
    {
        // 実装意図: 攻撃中は移動を止め、攻撃パターンと Animator の開始/終了を Controller が同期する。
        ChangeState(EnemyState.Attack);
        StopMovement();
        animationDriver.BeginAttack(attackPattern.AttackKind);

        yield return StartCoroutine(attackPattern.Attack());

        animationDriver.EndAttack(attackPattern.AttackKind);
        runningAttack = null;

        if(State == EnemyState.Attack)
        {
            ChangeState(EnemyState.Idle);
        }
    }

    void EnterHitStun()
    {
        if(State == EnemyState.Dead) return;

        // 実装意図: 被弾は攻撃より優先して割り込み、既存攻撃 coroutine の残り処理を走らせない。
        CancelAttack();
        StopMovement();

        if(hitStunRoutine != null)
        {
            StopCoroutine(hitStunRoutine);
        }

        hitStunRoutine = StartCoroutine(HitStun());
    }

    IEnumerator HitStun()
    {
        ChangeState(EnemyState.HitStun);

        float stunTime = Definition != null ? Definition.HitStunTime : 0.12f;
        if(stunTime > 0f)
        {
            yield return new WaitForSeconds(stunTime);
        }

        hitStunRoutine = null;

        if(State == EnemyState.HitStun)
        {
            ChangeState(EnemyState.Idle);
        }
    }

    void CancelAttack()
    {
        if(runningAttack != null)
        {
            StopCoroutine(runningAttack);
            runningAttack = null;
        }

        for(int i = 0; i < attackPatterns.Count; i++)
        {
            attackPatterns[i].Cancel();
        }
    }

    void StopMovement()
    {
        if(movement != null)
        {
            movement.Stop();
        }
    }
}
