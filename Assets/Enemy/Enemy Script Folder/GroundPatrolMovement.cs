using System.Collections;
using UnityEngine;

// 実装意図: Bink/Skaley の既存地上巡回を IEnemyMovement として再利用し、崖前ジャンプもここに閉じ込める。
public class GroundPatrolMovement : MonoBehaviour, IEnemyMovement
{
    // 実装意図: 移行済み prefab は scene override を優先できるよう、Definition 上書きを個別に切れる。
    [SerializeField] protected bool useDefinitionSettings = true;
    [SerializeField] protected float MoveSpeed = 2f;
    [SerializeField] protected float Dirx = -1f;
    [SerializeField] protected EnemyController enemy;
    [SerializeField] protected float JumpImpulse = 5f;
    [SerializeField] protected float JumpPower = 0.6f;
    [SerializeField] protected float JumpSlow = 2f;
    [SerializeField] protected float MaxHeight = 0.7f;

    protected Rigidbody2D rb;
    protected float JumpstartY;
    protected bool Jumping;
    protected bool JumpingCharge;
    protected EnemyAnimationDriver animationDriver;

    Coroutine jumpChargeRoutine;

    public virtual void Initialize(EnemyController controller)
    {
        enemy = controller;
        rb = GetComponent<Rigidbody2D>();
        animationDriver = controller != null ? controller.Animation : null;
        if(useDefinitionSettings)
        {
            ApplyDefinition(controller != null ? controller.Definition : null);
        }
        JumpingCharge = false;
        Jumping = false;
    }

    public virtual bool Tick()
    {
        // 実装意図: Update 側では「動いたか」だけを返し、状態名の決定は EnemyController に任せる。
        if(enemy == null || rb == null)
        {
            return false;
        }

        if(!enemy.IsGrounded)
        {
            return Jumping;
        }

        if(enemy.IsAtLedge)
        {
            // 実装意図: 崖検知直後に即ジャンプせず、既存の予備動作アニメーション時間を残す。
            if(!Jumping && !JumpingCharge)
            {
                jumpChargeRoutine = StartCoroutine(JumpCharge());
            }

            return false;
        }

        MoveX(Dirx);
        return Mathf.Abs(Dirx) > 0f;
    }

    public virtual void FixedTick()
    {
        // 実装意図: ジャンプの上昇補助は物理更新に寄せ、巡回の横移動とは独立して調整する。
        if(!Jumping || rb == null)
        {
            return;
        }

        float startY = JumpstartY + Mathf.Min(JumpSlow, MaxHeight);
        float endY = JumpstartY + Mathf.Max(JumpSlow, MaxHeight);

        if(rb.velocity.y < 0f)
        {
            return;
        }

        if(rb.position.y < startY)
        {
            rb.AddForce(Vector2.up * JumpPower, ForceMode2D.Force);
        }
        else if(rb.position.y < endY)
        {
            float t = Mathf.InverseLerp(startY, endY, rb.position.y);
            float thrust = Mathf.Lerp(JumpPower, 0f, t);
            rb.AddForce(Vector2.up * thrust, ForceMode2D.Force);
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Min(rb.velocity.y, 0f));
        }
    }

    public virtual void Stop()
    {
        if(rb != null)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        SetWalk(false);
    }

    protected virtual void MoveX(float directionX)
    {
        SetJump(false);
        SetWalk(true);
        rb.velocity = new Vector2(directionX * MoveSpeed, rb.velocity.y);
    }

    protected virtual void Jump(float directionX)
    {
        if(rb == null)
        {
            return;
        }

        Jumping = true;
        JumpstartY = rb.position.y;
        SetJump(true);
        SetJumpCharge(false);
        rb.AddForce(Vector2.up * JumpImpulse, ForceMode2D.Impulse);
        rb.velocity = new Vector2(directionX * MoveSpeed, rb.velocity.y);
    }

    protected virtual IEnumerator JumpCharge()
    {
        if(rb == null || enemy == null)
        {
            yield break;
        }

        SetWalk(false);
        JumpingCharge = true;
        SetJumpCharge(true);
        rb.velocity = new Vector2(0f, rb.velocity.y);

        float chargeTime = DefinitionGroundPatrol().jumpChargeTime;
        if(chargeTime > 0f)
        {
            yield return new WaitForSeconds(chargeTime);
        }

        JumpingCharge = false;

        if(!enemy.IsGrounded || !enemy.IsAtLedge)
        {
            SetJumpCharge(false);
            yield break;
        }

        Jump(Dirx);

        float postJumpLock = DefinitionGroundPatrol().postJumpLockTime;
        if(postJumpLock > 0f)
        {
            yield return new WaitForSeconds(postJumpLock);
        }

        jumpChargeRoutine = null;
    }

    protected virtual void ApplyDefinition(EnemyDefinition definition)
    {
        // 実装意図: 新規敵では EnemyDefinition を正とし、既存敵では useDefinitionSettings で旧値を守る。
        if(definition == null) return;

        EnemyDefinition.GroundPatrolSettings settings = definition.GroundPatrol;
        if(settings == null) return;

        MoveSpeed = settings.moveSpeed;
        Dirx = settings.directionX;
        JumpImpulse = settings.jumpImpulse;
        JumpPower = settings.jumpPower;
        JumpSlow = settings.jumpSlow;
        MaxHeight = settings.maxHeight;
    }

    EnemyDefinition.GroundPatrolSettings DefinitionGroundPatrol()
    {
        if(enemy != null && enemy.Definition != null && enemy.Definition.GroundPatrol != null)
        {
            return enemy.Definition.GroundPatrol;
        }

        return new EnemyDefinition.GroundPatrolSettings();
    }

    protected void SetWalk(bool value)
    {
        if(animationDriver != null)
        {
            if(value) animationDriver.ApplyState(EnemyState.Move);
            else animationDriver.ApplyState(EnemyState.Idle);
        }
    }

    protected void SetJump(bool value)
    {
        if(animationDriver != null)
        {
            animationDriver.SetJump(value);
        }
    }

    protected void SetJumpCharge(bool value)
    {
        if(animationDriver != null)
        {
            animationDriver.SetJumpCharge(value);
        }
    }

    protected virtual void OnDisable()
    {
        if(jumpChargeRoutine != null)
        {
            StopCoroutine(jumpChargeRoutine);
            jumpChargeRoutine = null;
        }
    }
}
