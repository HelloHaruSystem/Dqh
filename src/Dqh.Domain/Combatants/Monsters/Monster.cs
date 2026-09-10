using Dqh.Domain.Abilities;
using Dqh.Domain.Combatants;
using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Magic;
using Dqh.Domain.Strategies;

namespace Dqh.Domain.Combatants.Monsters;

/// <summary>
/// A single generic monster battle entity, parameterized by a <see cref="MonsterDefinition"/>
/// looked up from <see cref="MonsterBestiary"/> rather than one subclass per monster kind.
/// </summary>
public sealed class Monster : IMonster, IAttacker, IFleeable
{
    private readonly HitPointTrack _hitPoints;
    private readonly MonsterDefinition _definition;
    private readonly ITargetSelectionStrategy _targetSelectionStrategy;

    private Monster(MonsterDefinition definition, ITargetSelectionStrategy targetSelectionStrategy)
    {
        _definition = definition;
        _targetSelectionStrategy = targetSelectionStrategy;
        _hitPoints = new HitPointTrack(definition.MaxHitPoints);
    }

    /// <param name="kind">Looked up in <see cref="MonsterBestiary"/>.</param>
    /// <param name="targetSelectionStrategy">
    /// Decides which living party member this monster attacks — defaults to
    /// <see cref="RandomTargetStrategy"/> when the caller doesn't need a specific one.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="kind"/> has no bestiary entry.</exception>
    public static Monster Create(MonsterKind kind, ITargetSelectionStrategy? targetSelectionStrategy = null) =>
        new(MonsterBestiary.Get(kind), targetSelectionStrategy ?? new RandomTargetStrategy());

    public string Name => _definition.Name;
    public MonsterKind Kind => _definition.Kind;
    public int AttackPower => _definition.AttackPower;
    public int DefensePower => _definition.DefensePower;
    public int Speed => _definition.Speed;

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

    /// <summary>Rolls against this monster's own attack/flee weights.</summary>
    public bool AttemptFlee()
    {
        var roll = Random.Shared.Next(_definition.AttackWeight + _definition.FleeWeight);
        return roll < _definition.FleeWeight;
    }

    /// <returns>The targeted party member, or <c>null</c> if nobody is left standing.</returns>
    public Adventurer? ChooseTarget(IReadOnlyList<Adventurer> availableTargets) =>
        availableTargets.Count == 0 ? null : _targetSelectionStrategy.SelectTarget(this, availableTargets);
}
