namespace Dqh.Domain.Combatants;

/// <summary>Anything that can take part in a battle, hero or monster alike.</summary>
public interface ICombatant
{
    string Name { get; }
    int MaxHitPoints { get; }
    int CurrentHitPoints { get; }
    bool IsDefeated { get; }

    /// <param name="amount">Non-negative damage to apply.</param>
    void TakeDamage(int amount);

    /// <param name="amount">Non-negative hit points to restore, clamped to <see cref="MaxHitPoints"/>.</param>
    void Heal(int amount);
}
