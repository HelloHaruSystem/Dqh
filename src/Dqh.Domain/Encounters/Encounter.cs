using System.Linq;
using Dqh.Domain.Combatants.Monsters;

namespace Dqh.Domain.Encounters;

/// <summary>A monster encounter on the overworld — a group of one or more monsters.</summary>
public sealed class Encounter
{
    public IReadOnlyList<IMonster> Monsters { get; }
    public bool IsResolved { get; private set; }

    /// <param name="monsters">Must contain at least one monster.</param>
    public Encounter(IEnumerable<IMonster> monsters)
    {
        var group = monsters.ToList();
        if (group.Count == 0)
            throw new ArgumentException("Encounter must include at least one monster.", nameof(monsters));

        Monsters = group;
    }

    public Encounter(IMonster monster) : this([monster])
    {
    }

    /// <summary>Marks the encounter as over.</summary>
    internal void Resolve() => IsResolved = true;
}
