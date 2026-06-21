using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationDriver
{
    readonly Animator animator;
    readonly EnemyDefinition.AnimationSettings settings;
    readonly HashSet<string> parameters = new HashSet<string>();

    public EnemyAnimationDriver(Animator animator, EnemyDefinition definition)
    {
        this.animator = animator;
        settings = definition != null ? definition.Animation : new EnemyDefinition.AnimationSettings();

        if(animator == null) return;

        foreach(AnimatorControllerParameter parameter in animator.parameters)
        {
            parameters.Add(parameter.name);
        }
    }

    public void ApplyState(EnemyState state)
    {
        if(state == EnemyState.Dead)
        {
            SetBool(settings.walkBool, false);
            SetBool(settings.shotBool, false);
            SetBool(settings.attackBool, false);
            SetBool(settings.missileBool, false);
            SetTrigger(settings.deathTrigger);
            return;
        }

        if(state == EnemyState.HitStun || state == EnemyState.Idle)
        {
            SetBool(settings.walkBool, false);
        }

        if(state == EnemyState.Move)
        {
            SetBool(settings.walkBool, true);
        }
    }

    public void BeginAttack(EnemyAttackKind kind)
    {
        SetBool(settings.walkBool, false);

        if(kind == EnemyAttackKind.Melee)
        {
            SetBool(settings.attackBool, true);
        }
        else if(kind == EnemyAttackKind.FanShot)
        {
            SetBool(settings.shotBool, true);
        }
        else if(kind == EnemyAttackKind.MissileBurst)
        {
            SetBool(settings.missileBool, true);
        }
    }

    public void EndAttack(EnemyAttackKind kind)
    {
        if(kind == EnemyAttackKind.Melee)
        {
            SetBool(settings.attackBool, false);
        }
        else if(kind == EnemyAttackKind.FanShot)
        {
            SetBool(settings.shotBool, false);
        }
        else if(kind == EnemyAttackKind.MissileBurst)
        {
            SetBool(settings.shotBool, false);
            SetTrigger(settings.missileFinishTrigger);
            SetBool(settings.missileBool, false);
        }
    }

    public void SetJump(bool value)
    {
        SetBool(settings.jumpBool, value);
    }

    public void SetJumpCharge(bool value)
    {
        SetBool(settings.jumpChargeBool, value);
    }

    public void SetShot(bool value)
    {
        SetBool(settings.shotBool, value);
    }

    void SetBool(string key, bool value)
    {
        if(CanUseParameter(key))
        {
            animator.SetBool(key, value);
        }
    }

    void SetTrigger(string key)
    {
        if(CanUseParameter(key))
        {
            animator.SetTrigger(key);
        }
    }

    bool CanUseParameter(string key)
    {
        return animator != null && !string.IsNullOrEmpty(key) && parameters.Contains(key);
    }
}
