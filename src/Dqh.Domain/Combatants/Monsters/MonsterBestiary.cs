using Dqh.Domain.Magic;

namespace Dqh.Domain.Combatants.Monsters;

/// <summary>
/// Central catalog of monster stat templates, keyed by <see cref="MonsterKind"/>.
/// The place to look up a monster's stats/weaknesses (e.g. "what is a Ghost weak to?")
/// without instantiating one.
/// </summary>
internal static class MonsterBestiary
{
    private static readonly Dictionary<MonsterKind, MonsterDefinition> Definitions = new()
    {
        [MonsterKind.Slime] = new MonsterDefinition(
            MonsterKind.Slime, "Slime",
            maxHitPoints: 12, attackPower: 3, defensePower: 1,
            attackElement: ElementType.Physical,
            weaknesses: [ElementType.Fire],
            attackDescriptionTemplate: "{0} bumps clumsily into {1}",
            attackWeight: 100, fleeWeight: 0),

        [MonsterKind.Dracky] = new MonsterDefinition(
            MonsterKind.Dracky, "Dracky",
            maxHitPoints: 10, attackPower: 4, defensePower: 1,
            attackElement: ElementType.Wind,
            weaknesses: [ElementType.Ice],
            attackDescriptionTemplate: "{0} swoops in and nips at {1}",
            attackWeight: 100, fleeWeight: 0),

        [MonsterKind.Ghost] = new MonsterDefinition(
            MonsterKind.Ghost, "Ghost",
            maxHitPoints: 16, attackPower: 5, defensePower: 2,
            attackElement: ElementType.Ice,
            weaknesses: [ElementType.Wind],
            attackDescriptionTemplate: "{0} reaches through {1} with an icy touch",
            attackWeight: 100, fleeWeight: 0),

        [MonsterKind.MetalSlime] = new MonsterDefinition(
            MonsterKind.MetalSlime, "Metal Slime",
            maxHitPoints: 4, attackPower: 2, defensePower: 50,
            attackElement: ElementType.Physical,
            weaknesses: [],
            attackDescriptionTemplate: "{0} weakly bonks {1}",
            attackWeight: 10, fleeWeight: 90),
    };

    /// <exception cref="ArgumentOutOfRangeException"><paramref name="kind"/> has no bestiary entry.</exception>
    public static MonsterDefinition Get(MonsterKind kind) =>
        Definitions.TryGetValue(kind, out var definition)
            ? definition
            : throw new ArgumentOutOfRangeException(nameof(kind), kind, "No bestiary entry for this monster kind.");
}
