using System.Collections;
using UnityEngine;

public abstract class EnemyBehaviourBase : MonoBehaviour, IEnemyBehaviour
{
    public bool IsInitialized { get; private set; }

    protected EnemyController Enemy { get; private set; }
    protected Transform Target { get { return Enemy != null ? Enemy.Target : null; } }
    protected Rigidbody2D Body { get; private set; }
    protected Collider2D BodyCollider { get; private set; }
    protected Animator CachedAnimator { get; private set; }
    protected SpriteRenderer CachedSpriteRenderer { get; private set; }
    protected AudioSource CachedAudioSource { get; private set; }

    public void Initialize(EnemyController controller)
    {
        Enemy = controller;

        if(IsInitialized)
        {
            OnControllerRefreshed();
            return;
        }

        Body = GetComponent<Rigidbody2D>();
        BodyCollider = GetComponent<Collider2D>();
        CachedAnimator = GetComponent<Animator>();
        CachedSpriteRenderer = GetComponent<SpriteRenderer>();
        CachedAudioSource = GetComponent<AudioSource>();
        IsInitialized = true;
        OnInitialized();
    }

    protected virtual void OnInitialized()
    {
    }

    protected virtual void OnControllerRefreshed()
    {
    }

    protected virtual void OnBehaviourEnabled()
    {
    }

    protected virtual void OnBehaviourDisabled()
    {
    }

    protected virtual void OnEnable()
    {
        if(IsInitialized)
        {
            OnBehaviourEnabled();
        }
    }

    protected virtual void OnDisable()
    {
        if(IsInitialized)
        {
            OnBehaviourDisabled();
        }
    }

    protected void StopBodyVelocity(bool preserveVerticalVelocity = false)
    {
        if(Body == null)
        {
            return;
        }

        Body.velocity = preserveVerticalVelocity
            ? new Vector2(0f, Body.velocity.y)
            : Vector2.zero;
    }

    protected static IEnumerator WaitIfPositive(float duration)
    {
        if(duration > 0f)
        {
            yield return new WaitForSeconds(duration);
        }
    }
}
