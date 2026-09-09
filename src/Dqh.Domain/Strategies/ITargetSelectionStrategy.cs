using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Combatants.Monsters;

namespace Dqh.Domain.Strategies;

/// <summary>Decides which living party member a monster's attack lands on.</summary>
public interface ITargetSelectionStrategy
{
    Adventurer SelectTarget(IMonster attacker, IReadOnlyList<Adventurer> availableTargets);
}
