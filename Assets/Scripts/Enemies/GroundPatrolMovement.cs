using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GroundPatrolMovement : EnemyBehaviourBase, IEnemyMovement
{
    const string GroundTag = "Ground";

    [FormerlySerializedAs("MoveSpeed")]
    [SerializeField] protected float moveSpeed = 2f;
    [FormerlySerializedAs("Dirx")]
    [SerializeField] protected float directionX = -1f;
    [FormerlySerializedAs("JumpImpulse")]
    [SerializeField] protected float jumpImpulse = 5f;
    [FormerlySerializedAs("JumpPower")]
    [SerializeField] protected float jumpPower = 0.6f;
    [FormerlySerializedAs("JumpSlow")]
    [SerializeField] protected float jumpSlowHeight = 2f;
    [FormerlySerializedAs("MaxHeight")]
    [SerializeField] protected float maxJumpHeight = 0.7f;
    [SerializeField] protected float jumpChargeTime = 0.5f;
    [SerializeField] protected float postJumpLockTime = 0.3f;

    float jumpStartY;
    bool isJumping;
    bool leftGroundAfterJump;
    bool isApplyingJumpAssist;
    bool isChargingJump;
    Coroutine jumpChargeRoutine;
    readonly HashSet<Collider2D> bodyGroundContacts = new HashSet<Collider2D>();

    protected override void OnInitialized()
    {
        isChargingJump = false;
        isJumping = false;
        leftGroundAfterJump = false;
        isApplyingJumpAssist = false;
    }

    public virtual bool Tick()
    {
        if(Enemy == null || Body == null)
        {
            return false;
        }

        bool isGrounded = Enemy.IsGrounded || IsBodyGrounded();
        bool isAtLedge = Enemy.IsAtLedge;

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
        if(!isJumping || !isApplyingJumpAssist || Body == null)
        {
            return;
        }

        if(Body.velocity.y <= 0f)
        {
            isApplyingJumpAssist = false;
            return;
        }

        float minHeight = jumpStartY + Mathf.Min(jumpSlowHeight, maxJumpHeight);
        float maxHeight = jumpStartY + Mathf.Max(jumpSlowHeight, maxJumpHeight);

        if(Body.position.y < minHeight)
        {
            Body.AddForce(Vector2.up * jumpPower, ForceMode2D.Force);
        }
        else if(Body.position.y < maxHeight)
        {
            float heightRate = Mathf.InverseLerp(minHeight, maxHeight, Body.position.y);
            float thrust = Mathf.Lerp(jumpPower, 0f, heightRate);
            Body.AddForce(Vector2.up * thrust, ForceMode2D.Force);
        }
        else
        {
            isApplyingJumpAssist = false;
            Body.velocity = new Vector2(Body.velocity.x, Mathf.Min(Body.velocity.y, 0f));
        }
    }

    public virtual void Stop()
    {
        StopBodyVelocity(true);
        SetWalk(false);
    }

    protected virtual void MoveX(float direction)
    {
        SetJump(false);
        SetWalk(true);
        Body.velocity = new Vector2(direction * moveSpeed, Body.velocity.y);
    }

    protected virtual void MoveAirborneX(float direction)
    {
        SetWalk(false);
        Body.velocity = new Vector2(direction * moveSpeed, Body.velocity.y);
    }

    protected virtual void Jump(float direction)
    {
        if(Body == null)
        {
            return;
        }

        isJumping = true;
        leftGroundAfterJump = false;
        isApplyingJumpAssist = true;
        jumpStartY = Body.position.y;
        SetWalk(false);
        SetJump(true);
        SetJumpCharge(false);
        Body.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
        Body.velocity = new Vector2(direction * moveSpeed, Body.velocity.y);
    }

    protected virtual IEnumerator JumpCharge()
    {
        if(Body == null || Enemy == null)
        {
            yield break;
        }

        SetWalk(false);
        isChargingJump = true;
        SetJumpCharge(true);
        Body.velocity = new Vector2(0f, Body.velocity.y);

        if(jumpChargeTime > 0f)
        {
            yield return new WaitForSeconds(jumpChargeTime);
        }

        isChargingJump = false;

        if(!Enemy.IsAtLedge)
        {
            SetJumpCharge(false);
            jumpChargeRoutine = null;
            yield break;
        }

        Jump(directionX);

        if(postJumpLockTime > 0f)
        {
            yield return new WaitForSeconds(postJumpLockTime);
        }

        jumpChargeRoutine = null;
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

        if(leftGroundAfterJump && Body.velocity.y <= 0.01f)
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
        if(isJumping && Body != null && Body.velocity.y <= 0.01f && IsGroundCollision(collision))
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

    protected void SetWalk(bool value)
    {
        if(Enemy != null && Enemy.Animation != null)
        {
            Enemy.Animation.ApplyState(value ? EnemyState.Move : EnemyState.Idle);
        }
    }

    protected void SetJump(bool value)
    {
        if(Enemy != null && Enemy.Animation != null)
        {
            Enemy.Animation.SetJump(value);
        }
    }

    protected void SetJumpCharge(bool value)
    {
        if(Enemy != null && Enemy.Animation != null)
        {
            Enemy.Animation.SetJumpCharge(value);
        }
    }

    protected override void OnDisable()
    {
        if(jumpChargeRoutine != null)
        {
            StopCoroutine(jumpChargeRoutine);
            jumpChargeRoutine = null;
        }

        base.OnDisable();
    }
}
