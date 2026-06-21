public readonly struct DamageResult
{
    public readonly bool Applied;
    public readonly bool Killed;
    public readonly int CurrentHealth;
    public readonly int MaxHealth;

    public DamageResult(bool applied, bool killed, int currentHealth, int maxHealth)
    {
        Applied = applied;
        Killed = killed;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
    }

    public static DamageResult Ignored(int currentHealth, int maxHealth)
    {
        return new DamageResult(false, false, currentHealth, maxHealth);
    }
}
