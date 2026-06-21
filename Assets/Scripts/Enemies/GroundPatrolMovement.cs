using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class GroundPatrolMovement : MonoBehaviour, IEnemyMovement
{
    [SerializeField] protected bool useDefinitionSettings = true;
    [FormerlySerializedAs("MoveSpeed")]
    [SerializeField] protected float moveSpeed = 2f;
    [FormerlySerializedAs("Dirx")]
    [SerializeField] protected float directionX = -1f;
    [SerializeField] protected EnemyController enemy;
    [FormerlySerializedAs("JumpImpulse")]
    [SerializeField] protected float jumpImpulse = 5f;
    [FormerlySerializedAs("JumpPower")]
    [SerializeField] protected float jumpPower = 0.6f;
    [FormerlySerializedAs("JumpSlow")]
    [SerializeField] protected float jumpSlowHeight = 2f;
    [FormerlySerializedAs("MaxHeight")]
    [SerializeField] protected float maxJumpHeight = 0.7f;

    protected Rigidbody2D body;
    protected EnemyAnimationDriver animationDriver;

    float jumpStartY;
    bool isJumping;
    bool isChargingJump;
    Coroutine jumpChargeRoutine;

    public virtual void Initialize(EnemyController controller)
    {
        enemy = controller;
        body = GetComponent<Rigidbody2D>();
        animationDriver = controller != null ? controller.Animation : null;

        if(useDefinitionSettings)
        {
            ApplyDefinition(controller != null ? controller.Definition : null);
        }

        isChargingJump = false;
        isJumping = false;
    }

    public virtual bool Tick()
    {
        if(enemy == null || body == null)
        {
            return false;
        }

        if(!enemy.IsGrounded)
        {
            return isJumping;
        }

        isJumping = false;

        if(enemy.IsAtLedge)
        {
            StartJumpChargeIfNeeded();
            return false;
        }

        MoveX(directionX);
        return Mathf.Abs(directionX) > 0f;
    }

    public virtual void FixedTick()
    {
        if(!isJumping || body == null || body.velocity.y < 0f)
        {
            return;
        }

        float minHeight = jumpStartY + Mathf.Min(jumpSlowHeight, maxJumpHeight);
        float maxHeight = jumpStartY + Mathf.Max(jumpSlowHeight, maxJumpHeight);

        if(body.position.y < minHeight)
        {
            body.AddForce(Vector2.up * jumpPower, ForceMode2D.Force);
        }
        else if(body.position.y < maxHeight)
        {
            float heightRate = Mathf.InverseLerp(minHeight, maxHeight, body.position.y);
            float thrust = Mathf.Lerp(jumpPower, 0f, heightRate);
            body.AddForce(Vector2.up * thrust, ForceMode2D.Force);
        }
        else
        {
            body.velocity = new Vector2(body.velocity.x, Mathf.Min(body.velocity.y, 0f));
        }
    }

    public virtual void Stop()
    {
        if(body != null)
        {
            body.velocity = new Vector2(0f, body.velocity.y);
        }

        SetWalk(false);
    }

    protected virtual void MoveX(float direction)
    {
        SetJump(false);
        SetWalk(true);
        body.velocity = new Vector2(direction * moveSpeed, body.velocity.y);
    }

    protected virtual void Jump(float direction)
    {
        if(body == null)
        {
            return;
        }

        isJumping = true;
        jumpStartY = body.position.y;
        SetJump(true);
        SetJumpCharge(false);
        body.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
        body.velocity = new Vector2(direction * moveSpeed, body.velocity.y);
    }

    protected virtual IEnumerator JumpCharge()
    {
        if(body == null || enemy == null)
        {
            yield break;
        }

        SetWalk(false);
        isChargingJump = true;
        SetJumpCharge(true);
        body.velocity = new Vector2(0f, body.velocity.y);

        float chargeTime = CurrentGroundPatrolSettings().jumpChargeTime;
        if(chargeTime > 0f)
        {
            yield return new WaitForSeconds(chargeTime);
        }

        isChargingJump = false;

        if(!enemy.IsGrounded || !enemy.IsAtLedge)
        {
            SetJumpCharge(false);
            jumpChargeRoutine = null;
            yield break;
        }

        Jump(directionX);

        float postJumpLock = CurrentGroundPatrolSettings().postJumpLockTime;
        if(postJumpLock > 0f)
        {
            yield return new WaitForSeconds(postJumpLock);
        }

        jumpChargeRoutine = null;
    }

    protected virtual void ApplyDefinition(EnemyDefinition definition)
    {
        if(definition == null || definition.GroundPatrol == null)
        {
            return;
        }

        EnemyDefinition.GroundPatrolSettings settings = definition.GroundPatrol;
        moveSpeed = settings.moveSpeed;
        directionX = settings.directionX;
        jumpImpulse = settings.jumpImpulse;
        jumpPower = settings.jumpPower;
        jumpSlowHeight = settings.jumpSlowHeight;
        maxJumpHeight = settings.maxJumpHeight;
    }

    void StartJumpChargeIfNeeded()
    {
        if(!isJumping && !isChargingJump && jumpChargeRoutine == null)
        {
            jumpChargeRoutine = StartCoroutine(JumpCharge());
        }
    }

    EnemyDefinition.GroundPatrolSettings CurrentGroundPatrolSettings()
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
            animationDriver.ApplyState(value ? EnemyState.Move : EnemyState.Idle);
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
