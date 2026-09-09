using Dqh.Domain.Combatants.Adventurers;

namespace Dqh.Domain.Strategies;

/// <summary>Decides who leads the party — the member shown on the overworld map.</summary>
public interface ILeaderSelectionStrategy
{
    Adventurer SelectLeader(IReadOnlyList<Adventurer> standingAdventurers);
}
