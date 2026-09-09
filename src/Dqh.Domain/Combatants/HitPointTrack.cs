namespace Dqh.Domain.Combatants;

/// <summary>
/// Hit-point bookkeeping shared by <see cref="Adventurer"/> and <see cref="Monster"/>.
/// Composed into both rather than inherited, since the two hierarchies are otherwise
/// deliberately independent (they only share the <see cref="ICombatant"/> capability).
/// </summary>
internal sealed class HitPointTrack
{
    public int MaxHitPoints { get; }
    public int Current { get; private set; }
    public bool IsDefeated => Current <= 0;

    /// <param name="maxHitPoints">Must be positive.</param>
    public HitPointTrack(int maxHitPoints)
    {
        if (maxHitPoints <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxHitPoints), "Max hit points must be positive.");

        MaxHitPoints = maxHitPoints;
        Current = maxHitPoints;
    }

    /// <param name="amount">Non-negative damage to apply.</param>
    public void TakeDamage(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Damage cannot be negative.");

        Current = Math.Max(0, Current - amount);
    }

    /// <param name="amount">Non-negative hit points to restore, clamped to <see cref="MaxHitPoints"/>.</param>
    public void Heal(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Heal amount cannot be negative.");

        Current = Math.Min(MaxHitPoints, Current + amount);
    }
}
