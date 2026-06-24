using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GroundPatrolMovement : MonoBehaviour, IEnemyMovement
{
    const string GroundTag = "Ground";

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
    bool leftGroundAfterJump;
    bool isApplyingJumpAssist;
    bool isChargingJump;
    Coroutine jumpChargeRoutine;
    readonly HashSet<Collider2D> bodyGroundContacts = new HashSet<Collider2D>();

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
        leftGroundAfterJump = false;
        isApplyingJumpAssist = false;
    }

    public virtual bool Tick()
    {
        if(enemy == null || body == null)
        {
            return false;
        }

        bool isGrounded = enemy.IsGrounded || IsBodyGrounded();
        bool isAtLedge = enemy.IsAtLedge;

        RefreshJumpState(isGrounded);

        if(!isJumping && isAtLedge)
        {
            StartJumpChargeIfNeeded();
            return false;
        }

        if(!isGrounded || isJumping)
        {
            MoveAirborneX(directionX);
            return false;
        }

        MoveX(directionX);
        return Mathf.Abs(directionX) > 0f;
    }

    public virtual void FixedTick()
    {
        if(!isJumping || !isApplyingJumpAssist || body == null)
        {
            return;
        }

        if(body.velocity.y <= 0f)
        {
            isApplyingJumpAssist = false;
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
            isApplyingJumpAssist = false;
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

    protected virtual void MoveAirborneX(float direction)
    {
        SetWalk(false);
        body.velocity = new Vector2(direction * moveSpeed, body.velocity.y);
    }

    protected virtual void Jump(float direction)
    {
        if(body == null)
        {
            return;
        }

        isJumping = true;
        leftGroundAfterJump = false;
        isApplyingJumpAssist = true;
        jumpStartY = body.position.y;
        SetWalk(false);
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

        if(!enemy.IsAtLedge)
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

    void RefreshJumpState(bool isGrounded)
    {
        if(!isJumping)
        {
            return;
        }

        if(!isGrounded)
        {
            leftGroundAfterJump = true;
            return;
        }

        if(leftGroundAfterJump && body.velocity.y <= 0.01f)
        {
            FinishJump();
        }
    }

    void FinishJump()
    {
        isJumping = false;
        leftGroundAfterJump = false;
        isApplyingJumpAssist = false;
        SetJump(false);
        SetJumpCharge(false);
        SetWalk(Mathf.Abs(directionX) > 0f);
    }

    bool IsBodyGrounded()
    {
        bodyGroundContacts.RemoveWhere(collision => collision == null || !collision.enabled);
        return bodyGroundContacts.Count > 0;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        RefreshBodyGroundContact(collision);
        TryFinishJumpFromGroundCollision(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        RefreshBodyGroundContact(collision);
        TryFinishJumpFromGroundCollision(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if(collision != null && collision.collider != null)
        {
            bodyGroundContacts.Remove(collision.collider);
        }
    }

    void RefreshBodyGroundContact(Collision2D collision)
    {
        if(collision == null || collision.collider == null)
        {
            return;
        }

        if(IsGroundCollision(collision))
        {
            bodyGroundContacts.Add(collision.collider);
        }
        else
        {
            bodyGroundContacts.Remove(collision.collider);
        }
    }

    void TryFinishJumpFromGroundCollision(Collision2D collision)
    {
        if(isJumping && body != null && body.velocity.y <= 0.01f && IsGroundCollision(collision))
        {
            FinishJump();
        }
    }

    bool IsGroundCollision(Collision2D collision)
    {
        return collision != null && collision.collider != null && collision.collider.CompareTag(GroundTag) && HasUpwardContact(collision);
    }

    bool HasUpwardContact(Collision2D collision)
    {
        for(int i = 0; i < collision.contactCount; i++)
        {
            if(collision.GetContact(i).normal.y > 0.3f)
            {
                return true;
            }
        }

        return false;
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
