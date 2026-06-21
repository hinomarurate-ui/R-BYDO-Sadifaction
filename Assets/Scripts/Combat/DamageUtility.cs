using UnityEngine;

public static class DamageUtility
{
    public static bool TryGetDamageable(Collider2D hit, out IDamageable damageable)
    {
        damageable = hit != null ? hit.GetComponentInParent<IDamageable>() : null;
        return damageable != null;
    }

    public static DamageResult ApplyDamage(IDamageable damageable, DamageRequest request, bool shakeOnKill)
    {
        if(damageable == null)
        {
            return DamageResult.Ignored(0, 0);
        }

        DamageResult result = damageable.TakeDamage(request);
        if(shakeOnKill && result.Killed && request.KillShakePower > 0f && request.KillShakeTime > 0f)
        {
            ScreenShake.Shake(request.KillShakePower, request.KillShakeTime);
        }

        return result;
    }

    public static bool TryApplyDamage(Collider2D hit, DamageRequest request, bool shakeOnKill, out DamageResult result)
    {
        if(!TryGetDamageable(hit, out IDamageable damageable))
        {
            result = DamageResult.Ignored(0, 0);
            return false;
        }

        result = ApplyDamage(damageable, request, shakeOnKill);
        return result.Applied;
    }
}
