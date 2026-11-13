using UnityEngine;

public class SkaleyFlyingMove : EnemyBehaviourBase, IEnemyMovement
{
    [SerializeField] float horizontalRange = 3f;
    [SerializeField] float verticalRange = 1f;
    [SerializeField] float horizontalSpeed = 1f;
    [SerializeField] float verticalSpeed = 1.4f;

    Vector2 centerPosition;
    float horizontalAngle;
    float verticalAngle;
    float originalGravityScale;
    RigidbodyConstraints2D originalConstraints;
    protected override void OnInitialized()
    {
        centerPosition = Body != null ? Body.position : (Vector2)transform.position;

        if(Body != null)
        {
            originalGravityScale = Body.gravityScale;
            originalConstraints = Body.constraints;
        }

        StartFloating();
    }

    // Update is called once per frame
    public bool Tick()
    {
        return Body != null;
    }

    public void FixedTick()
    {
        if(Body == null)
        {
            return;
        }

        horizontalAngle += horizontalSpeed * Time.fixedDeltaTime;
        verticalAngle += verticalSpeed * Time.fixedDeltaTime;

        Vector2 offset = new Vector2(
            Mathf.Sin(horizontalAngle) * horizontalRange,
            Mathf.Sin(verticalAngle) * verticalRange);

        Body.MovePosition(centerPosition + offset);
    }

    public void Stop()
    {
        StopBodyVelocity();
    }

    protected override void OnBehaviourEnabled()
    {
        StartFloating();
    }

    protected override void OnBehaviourDisabled()
    {
        if(Body != null)
        {
            Body.velocity = Vector2.zero;
        }
    }

    void StartFloating()
    {
        if(Body == null)
        {
            return;
        }

        Body.gravityScale = 0f;
        Body.constraints = originalConstraints | RigidbodyConstraints2D.FreezeRotation;
        Body.velocity = Vector2.zero;
        
    }
}
