using Dqh.Domain.Combatants;
using Dqh.Domain.Magic;

namespace Dqh.Domain.Combatants.Monsters;

/// <summary>A monster: an <see cref="ICombatant"/> with battle stats and elemental weaknesses.</summary>
public interface IMonster : ICombatant
{
    MonsterKind Kind { get; }
    int AttackPower { get; }
    int DefensePower { get; }

    bool IsWeakTo(ElementType element);
}
