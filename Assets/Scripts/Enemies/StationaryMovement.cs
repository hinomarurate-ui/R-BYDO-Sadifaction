using UnityEngine;

public class StationaryMovement : EnemyBehaviourBase, IEnemyMovement
{
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
        StopBodyVelocity(true);
    }
}
