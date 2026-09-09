using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Combatants.Monsters;

namespace Dqh.Domain.Strategies;

/// <summary>Targets a uniformly random living party member.</summary>
public sealed class RandomTargetStrategy : ITargetSelectionStrategy
{
    public Adventurer SelectTarget(IMonster attacker, IReadOnlyList<Adventurer> availableTargets) =>
        availableTargets[Random.Shared.Next(availableTargets.Count)];
}
