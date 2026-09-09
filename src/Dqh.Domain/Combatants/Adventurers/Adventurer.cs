using Dqh.Domain.Combatants;

namespace Dqh.Domain.Combatants.Adventurers;

/// <summary>A party member with encapsulated hit points and mana (0-100).</summary>
public abstract class Adventurer : ICombatant
{
    private const int MinMana = 0;
    private const int MaxMana = 100;

    private readonly HitPointTrack _hitPoints;
    private int _mana;

    /// <param name="name">Must not be blank.</param>
    /// <param name="maxHitPoints">Must be positive.</param>
    /// <param name="startingMana">Clamped into the valid 0-100 range.</param>
    /// <param name="speed">How early this adventurer acts in a battle round — higher goes first.</param>
    protected Adventurer(string name, int maxHitPoints, int startingMana, int speed)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Adventurer must have a name.", nameof(name));

        Name = name;
        _hitPoints = new HitPointTrack(maxHitPoints);
        Mana = startingMana;
        Speed = speed;
    }

    public string Name { get; }

    public int MaxHitPoints => _hitPoints.MaxHitPoints;
    public int CurrentHitPoints => _hitPoints.Current;
    public bool IsDefeated => _hitPoints.IsDefeated;
    public int Speed { get; }

    /// <summary>Whether this adventurer chose to guard this round — halves the next hit they take.</summary>
    public bool IsGuarding { get; private set; }

    /// <summary>Current mana, always within [0, 100] regardless of what callers pass in.</summary>
    public int Mana
    {
        get => _mana;
        private set => _mana = Math.Clamp(value, MinMana, MaxMana);
    }

    public void TakeDamage(int amount) => _hitPoints.TakeDamage(IsGuarding ? amount / 2 : amount);

    public void Heal(int amount) => _hitPoints.Heal(amount);

    /// <summary>Marks this adventurer as guarding — halves the next hit they take, until <see cref="ResetGuard"/>.</summary>
    protected void SetGuarding() => IsGuarding = true;

    /// <summary>Clears the guarding state, e.g. at the start of a new battle round.</summary>
    public void ResetGuard() => IsGuarding = false;

    public bool CanAfford(int manaCost) => manaCost <= Mana;

    /// <param name="amount">Non-negative mana to spend; clamped at 0 if it exceeds the current amount.</param>
    public void SpendMana(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Mana cost cannot be negative.");

        Mana -= amount;
    }

    /// <param name="amount">Non-negative mana to restore, clamped at 100.</param>
    public void RestoreMana(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Restore amount cannot be negative.");

        Mana += amount;
    }

    /// <summary>Fully restores hit points and mana — an inn's rest.</summary>
    public void FullyRestore()
    {
        Heal(MaxHitPoints);
        RestoreMana(MaxMana);
    }

    /// <param name="target">Who the action affects.</param>
    /// <returns>A description of what happened — or that this adventurer is defeated and can't act.</returns>
    public string TakeTurn(ICombatant target) =>
        IsDefeated ? $"{Name} is defeated and can't act." : PerformTurnAction(target);

    /// <summary>Each concrete adventurer's own way of acting on their turn.</summary>
    protected abstract string PerformTurnAction(ICombatant target);
}
