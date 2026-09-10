using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Encounters;

namespace Dqh.Domain.Party;

public interface IParty
{
    IReadOnlyList<Adventurer> Members { get; }

    void Register(Adventurer adventurer);

    void Report(Encounter encounter);

    void ResolveEncounter(Encounter encounter, Action<Encounter> onResolved);

    Adventurer? FindAvailableHealer();

    Encounter? FindFirstUnresolvedEncounter();
}
