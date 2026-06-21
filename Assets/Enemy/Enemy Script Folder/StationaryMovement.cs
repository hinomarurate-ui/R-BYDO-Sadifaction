using UnityEngine;

// 実装意図: Pstaff のような動かない敵も IEnemyMovement として扱い、Controller 側の分岐を増やさない。
public class StationaryMovement : MonoBehaviour, IEnemyMovement
{
    protected EnemyController enemy;
    protected Rigidbody2D rb;

    public virtual void Initialize(EnemyController controller)
    {
        enemy = controller;
        rb = GetComponent<Rigidbody2D>();
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
        if(rb != null)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }
}
