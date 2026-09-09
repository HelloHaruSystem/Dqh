using Dqh.Domain.Combatants;
using Dqh.Domain.Magic;

namespace Dqh.Domain.Abilities;

/// <summary>Knows a list of offensive spells and can cast them.</summary>
public interface ISpellcaster
{
    IReadOnlyList<DamageSpell> KnownSpells { get; }

    /// <param name="spell">Must be one of <see cref="KnownSpells"/>.</param>
    /// <returns>Flavor text describing the effect.</returns>
    string Cast(DamageSpell spell, ICombatant target);
}
