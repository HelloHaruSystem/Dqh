using Dqh.Domain.Abilities;
using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Encounters;
using Dqh.Domain.Exceptions;
using Dqh.Domain.Utilities;

namespace Dqh.Domain.Party;

/// <summary>The player's party: tracks registered adventurers and the encounters they've faced.</summary>
public sealed class Party : IParty
{
    private readonly List<Adventurer> _roster = [];
    private readonly List<Encounter> _encounterLog = [];

    public IReadOnlyList<Adventurer> Members => _roster;

    /// <exception cref="AdventurerAlreadyRegisteredException"><paramref name="adventurer"/> is already registered.</exception>
    public void Register(Adventurer adventurer)
    {
        if (_roster.Contains(adventurer))
            throw new AdventurerAlreadyRegisteredException($"{adventurer.Name} is already in the party.");

        _roster.Add(adventurer);
    }

    public void Report(Encounter encounter)
    {
        if (!_encounterLog.Contains(encounter))
            _encounterLog.Add(encounter);
    }

    public void ResolveEncounter(Encounter encounter, Action<Encounter> onResolved)
    {
        encounter.Resolve();
        onResolved(encounter);
    }

    public Adventurer? FindAvailableHealer() =>
        DomainToolbox.FindFirst(_roster, a => !a.IsDefeated && a is IHealer);

    public Encounter? FindFirstUnresolvedEncounter() =>
        DomainToolbox.FindFirst(_encounterLog, e => !e.IsResolved);
}
