using System.Collections.Generic;
using UnityEngine;

// 実装意図: 既存 Animator Controller は変更せず、EnemyState と攻撃種別を既存パラメータ操作へ橋渡しする。
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

        // 実装意図: 敵ごとに Animator パラメータが違っても、存在しないキー操作で例外を出さない。
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
        // 実装意図: 古い Controller と新しい Definition を混在させても missing parameter を無視して動かす。
        return animator != null && !string.IsNullOrEmpty(key) && parameters.Contains(key);
    }
}
