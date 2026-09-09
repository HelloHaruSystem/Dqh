using Dqh.Domain.Magic;

namespace Dqh.Domain.Combatants;

/// <summary>
/// Immutable stat template for a <see cref="MonsterKind"/>, looked up from
/// <see cref="MonsterBestiary"/> and used to build live <see cref="Monster"/> instances.
/// </summary>
public sealed class MonsterDefinition
{
    public MonsterKind Kind { get; }
    public string Name { get; }
    public int MaxHitPoints { get; }
    public int AttackPower { get; }
    public int DefensePower { get; }
    public ElementType AttackElement { get; }
    public IReadOnlySet<ElementType> Weaknesses { get; }
    public string AttackDescriptionTemplate { get; }

    /// <param name="attackDescriptionTemplate">Flavor text with {0}=attacker name, {1}=target name.</param>
    public MonsterDefinition(
        MonsterKind kind,
        string name,
        int maxHitPoints,
        int attackPower,
        int defensePower,
        ElementType attackElement,
        IEnumerable<ElementType> weaknesses,
        string attackDescriptionTemplate)
    {
        Kind = kind;
        Name = name;
        MaxHitPoints = maxHitPoints;
        AttackPower = attackPower;
        DefensePower = defensePower;
        AttackElement = attackElement;
        Weaknesses = weaknesses.ToHashSet();
        AttackDescriptionTemplate = attackDescriptionTemplate;
    }
}
