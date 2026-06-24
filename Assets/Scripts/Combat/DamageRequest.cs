using UnityEngine;

public readonly struct DamageRequest
{
    public readonly int Amount;
    public readonly GameObject Source;
    public readonly Vector2 HitPoint;
    public readonly float KillShakePower;
    public readonly float KillShakeTime;

    public DamageRequest(int amount, GameObject source, Vector2 hitPoint, float killShakePower, float killShakeTime)
    {
        Amount = amount;
        Source = source;
        HitPoint = hitPoint;
        KillShakePower = killShakePower;
        KillShakeTime = killShakeTime;
    }

    public DamageRequest(int amount, GameObject source)
        : this(amount, source, Vector2.zero, 0f, 0f)
    {
    }
}
