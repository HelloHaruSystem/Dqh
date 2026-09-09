using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Combatants.Monsters;
using Dqh.Domain.Encounters;

namespace Dqh.Domain.Party;

public interface IParty
{
    void Register(Adventurer adventurer);

    void Report(Encounter encounter);

    /// <returns>The targeted adventurer, or <c>null</c> if the whole party is defeated.</returns>
    Adventurer? ChooseTarget(IMonster attacker);

    void ResolveEncounter(Encounter encounter, Action<Encounter> onResolved);

    Adventurer? FindAvailableHealer();

    Encounter? FindFirstUnresolvedEncounter();
}
