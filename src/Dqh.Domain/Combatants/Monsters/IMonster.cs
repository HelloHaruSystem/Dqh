using Dqh.Domain.Combatants;
using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Magic;

namespace Dqh.Domain.Combatants.Monsters;

/// <summary>A monster: an <see cref="ICombatant"/> with battle stats and elemental weaknesses.</summary>
public interface IMonster : ICombatant
{
    MonsterKind Kind { get; }
    int AttackPower { get; }
    int DefensePower { get; }

    bool IsWeakTo(ElementType element);

    /// <returns>The targeted party member, or <c>null</c> if nobody is left standing.</returns>
    Adventurer? ChooseTarget(IReadOnlyList<Adventurer> availableTargets);
}
