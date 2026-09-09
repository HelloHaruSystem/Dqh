using Dqh.Domain.Abilities;
using Dqh.Domain.Magic;

namespace Dqh.Domain.Combatants;

/// <summary>
/// A single generic monster battle entity, parameterized by a <see cref="MonsterDefinition"/>
/// looked up from <see cref="MonsterBestiary"/> rather than one subclass per monster kind.
/// </summary>
public sealed class Monster : IMonster, IAttacker
{
    private readonly HitPointTrack _hitPoints;
    private readonly MonsterDefinition _definition;

    private Monster(MonsterDefinition definition)
    {
        _definition = definition;
        _hitPoints = new HitPointTrack(definition.MaxHitPoints);
    }

    /// <param name="kind">Looked up in <see cref="MonsterBestiary"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="kind"/> has no bestiary entry.</exception>
    public static Monster Create(MonsterKind kind) => new(MonsterBestiary.Get(kind));

    public string Name => _definition.Name;
    public MonsterKind Kind => _definition.Kind;
    public int AttackPower => _definition.AttackPower;
    public int DefensePower => _definition.DefensePower;

    public int MaxHitPoints => _hitPoints.MaxHitPoints;
    public int CurrentHitPoints => _hitPoints.Current;
    public bool IsDefeated => _hitPoints.IsDefeated;

    public void TakeDamage(int amount) => _hitPoints.TakeDamage(amount);
    public void Heal(int amount) => _hitPoints.Heal(amount);

    public bool IsWeakTo(ElementType element) => _definition.Weaknesses.Contains(element);

    public string PerformAttack(ICombatant target)
    {
        target.TakeDamage(AttackPower);
        var description = string.Format(_definition.AttackDescriptionTemplate, Name, target.Name);
        return $"{description}! ({AttackPower} damage)";
    }
}
