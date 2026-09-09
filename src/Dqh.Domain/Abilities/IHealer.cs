using Dqh.Domain.Combatants;
using Dqh.Domain.Magic;

namespace Dqh.Domain.Abilities;

/// <summary>Knows a list of restorative spells and can cast them.</summary>
public interface IHealer
{
    IReadOnlyList<HealingSpell> KnownHealingSpells { get; }

    /// <param name="spell">Must be one of <see cref="KnownHealingSpells"/>.</param>
    /// <returns>Flavor text describing the effect.</returns>
    string Heal(HealingSpell spell, ICombatant target);
}
