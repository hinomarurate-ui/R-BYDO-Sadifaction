using UnityEngine;

public class StationaryMovement : MonoBehaviour, IEnemyMovement
{
    protected EnemyController enemy;
    protected Rigidbody2D body;

    public virtual void Initialize(EnemyController controller)
    {
        enemy = controller;
        body = GetComponent<Rigidbody2D>();
    }

    public virtual bool Tick()
    {
        Stop();
        return false;
    }

    public virtual void FixedTick()
    {
        Stop();
    }

    public virtual void Stop()
    {
        if(body != null)
        {
            body.velocity = new Vector2(0f, body.velocity.y);
        }
    }
}
