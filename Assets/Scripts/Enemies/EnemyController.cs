using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyController : MonoBehaviour
{
    [SerializeField] EnemyDefinition definition;
    [FormerlySerializedAs("ground")]
    [SerializeField] GroundSensor groundSensor;
    [FormerlySerializedAs("gake")]
    [SerializeField] LedgeSensor ledgeSensor;
    [FormerlySerializedAs("Status")]
    [SerializeField] EnemyDefinition fallbackDefinition;
    [FormerlySerializedAs("HP")]
    [SerializeField] EnemyHealth health;
    [FormerlySerializedAs("Move")]
    [SerializeField] MonoBehaviour movementOverride;

    [Header("Debug")]
    [FormerlySerializedAs("isGround")]
    [SerializeField] bool isGroundedDebug;
    [FormerlySerializedAs("isGake")]
    [SerializeField] bool isAtLedgeDebug;

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
        if(animationDriver != null)
        {
            animationDriver.ApplyState(State);
        }
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
        if(State == EnemyState.Dead)
        {
            return;
        }

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

    void InitializeRuntime()
    {
        if(initialized)
        {
            return;
        }

        initialized = true;
        Definition = definition != null ? definition : fallbackDefinition;
        sensor.Initialize(transform, groundSensor, ledgeSensor);
        animationDriver = new EnemyAnimationDriver(GetComponent<Animator>(), Definition);

        ResolveTarget();
        ResolveHealth();
        ResolveMovement();
        ResolveAttackPatterns();
        ResolveRoutines();
        RefreshSensorState();
    }

    void ResolveTarget()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        Target = playerObject != null ? playerObject.transform : null;
    }

    void ResolveHealth()
    {
        if(health == null)
        {
            health = GetComponent<EnemyHealth>();
        }

        if(health != null)
        {
            health.Initialize(this);
        }
    }

    void ResolveMovement()
    {
        movement = movementOverride as IEnemyMovement;
        if(movement == null)
        {
            movement = FindBehaviour<IEnemyMovement>();
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
            if(behaviour is IEnemyAttackPattern attackPattern)
            {
                attackPattern.Initialize(this);
                attackPatterns.Add(attackPattern);
            }
        }
    }

    void ResolveRoutines()
    {
        routines.Clear();

        foreach(MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
        {
            if(behaviour is IEnemyRoutine routine)
            {
                routine.Initialize(this);
                routines.Add(routine);
            }
        }
    }

    T FindBehaviour<T>() where T : class
    {
        foreach(MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
        {
            if(behaviour == this)
            {
                continue;
            }

            if(behaviour is T candidate)
            {
                return candidate;
            }
        }

        return null;
    }

    void RefreshSensorState()
    {
        sensor.Refresh();
        isGroundedDebug = sensor.IsGrounded;
        isAtLedgeDebug = sensor.IsAtLedge;
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
            if(!attackPatterns[i].CanAttack())
            {
                continue;
            }

            runningAttack = StartCoroutine(RunAttack(attackPatterns[i]));
            return true;
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
        if(State == EnemyState.Dead)
        {
            return;
        }

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
