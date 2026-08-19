using UnityEngine;

public class SkaleyFlyingMove : MonoBehaviour, IEnemyMovement
{
    [SerializeField] float horizontalRange = 3f;
    [SerializeField] float verticalRange = 1f;
    [SerializeField] float horizontalSpeed = 1f;
    [SerializeField] float verticalSpeed = 1.4f;

    Rigidbody2D body;
    Vector2 centerPosition;
    float horizontalAngle;
    float verticalAngle;
    float originalGravityScale;
    RigidbodyConstraints2D originalConstraints;
    bool initialized;

    public void Initialize(EnemyController controller)
    {
        body = GetComponent<Rigidbody2D>();
        centerPosition = body != null ? body.position : (Vector2)transform.position;

        if(body != null && !initialized)
        {
            originalGravityScale = body.gravityScale;
            originalConstraints = body.constraints;
        }        

        initialized = true;
        StartFloating();
    }

    // Update is called once per frame
    public bool Tick()
    {
        return body != null;
    }

    public void FixedTick()
    {
        if(body == null)
        {
            return;
        }

        horizontalAngle += horizontalSpeed * Time.fixedDeltaTime;
        verticalAngle += verticalSpeed * Time.fixedDeltaTime;

        Vector2 offset = new Vector2(
            Mathf.Sin(horizontalAngle) * horizontalRange,
            Mathf.Sin(verticalAngle) * verticalRange);

            body.MovePosition(centerPosition + offset);
    }

    public void Stop()
    {
        if(body != null)
        {
            body.velocity = Vector2.zero;
        }
    }

    void OnEnable()
    {
        StartFloating();
    }

    void OnDisable()
    {
        if(body != null && !initialized)
        {
            originalGravityScale = body.gravityScale;
            originalConstraints = body.constraints;
        }  
    }

    void StartFloating()
    {
        if(body == null)
        {
            return;
        }

        body.gravityScale = 0f;
        body.constraints = originalConstraints | RigidbodyConstraints2D.FreezeRotation;
        body.velocity = Vector2.zero;
        
    }
}
