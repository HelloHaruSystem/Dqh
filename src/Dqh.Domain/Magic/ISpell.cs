using Dqh.Domain.Combatants;

namespace Dqh.Domain.Magic;

/// <summary>
/// A single named spell. Data-driven rather than one type per spell name, so a
/// tier like Frizz/Frizzle/Kafrizz is just three instances of the same
/// implementation with rising power and mana cost.
/// </summary>
public interface ISpell
{
    string Name { get; }
    int ManaCost { get; }

    /// <param name="caster">Whoever is casting the spell (for flavor text).</param>
    /// <param name="target">Who the spell affects.</param>
    /// <returns>Flavor text describing the effect.</returns>
    string Apply(ICombatant caster, ICombatant target);
}
