using Dqh.Domain.Combatants;

namespace Dqh.Domain.Combatants.Adventurers;

/// <summary>
/// A party member. Encapsulates hit points and mana (0-100, invalid values are
/// clamped rather than accepted) and defines the polymorphism hook every concrete
/// adventurer type must implement differently.
/// </summary>
public abstract class Adventurer : ICombatant
{
    private const int MinMana = 0;
    private const int MaxMana = 100;

    private readonly HitPointTrack _hitPoints;
    private int _mana;

    /// <param name="name">Must not be blank.</param>
    /// <param name="maxHitPoints">Must be positive.</param>
    /// <param name="startingMana">Clamped into the valid 0-100 range.</param>
    protected Adventurer(string name, int maxHitPoints, int startingMana)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Adventurer must have a name.", nameof(name));

        Name = name;
        _hitPoints = new HitPointTrack(maxHitPoints);
        Mana = startingMana;
    }

    public string Name { get; }

    public int MaxHitPoints => _hitPoints.MaxHitPoints;
    public int CurrentHitPoints => _hitPoints.Current;
    public bool IsDefeated => _hitPoints.IsDefeated;

    /// <summary>Current mana, always within [0, 100] regardless of what callers pass in.</summary>
    public int Mana
    {
        get => _mana;
        private set => _mana = Math.Clamp(value, MinMana, MaxMana);
    }

    public void TakeDamage(int amount) => _hitPoints.TakeDamage(amount);

    public void Heal(int amount) => _hitPoints.Heal(amount);

    /// <param name="amount">Non-negative mana to spend; clamped at 0 if it exceeds the current amount.</param>
    public void SpendMana(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Mana cost cannot be negative.");

        // Deliberately permissive for now: this will start throwing
        // InsufficientManaException once kernekrav e) lands.
        Mana -= amount;
    }

    /// <param name="amount">Non-negative mana to restore, clamped at 100.</param>
    public void RestoreMana(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Restore amount cannot be negative.");

        Mana += amount;
    }

    /// <summary>Each concrete adventurer's unique battle action. This is the assignment's polymorphism proof.</summary>
    /// <param name="target">Who the move affects.</param>
    /// <returns>A flavor-text description of what happened.</returns>
    public abstract string UseSignatureMove(ICombatant target);
}
