using Dqh.Domain.Combatants.Monsters;

namespace Dqh.Domain.Encounters;

/// <summary>Picks 1-3 random monsters from the whole bestiary for a fresh encounter.</summary>
public sealed class RandomEncounterGenerator : IEncounterGenerator
{
    private const int MinGroupSize = 1;
    private const int MaxGroupSize = 3;

    private static readonly MonsterKind[] Pool = Enum.GetValues<MonsterKind>();

    public Encounter Generate()
    {
        var groupSize = Random.Shared.Next(MinGroupSize, MaxGroupSize + 1);
        var monsters = Enumerable.Range(0, groupSize)
            .Select(_ => Monster.Create(Pool[Random.Shared.Next(Pool.Length)]));

        return new Encounter(monsters);
    }
}
