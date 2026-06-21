using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] EnemyDefinition definition;

    [SerializeField] protected GroundSensor ground;
    [SerializeField] protected LedgeSensor gake;
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

