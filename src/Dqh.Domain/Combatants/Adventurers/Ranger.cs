using Dqh.Domain.Abilities;
using Dqh.Domain.Combatants;

namespace Dqh.Domain.Combatants.Adventurers;

/// <summary>Ranged attacker who can disengage from battle when things go wrong.</summary>
public sealed class Ranger : Adventurer, IAttacker, IFleeable
{
    private const int MaxHitPointsValue = 30;
    private const int StartingMana = 15;
    private const int AttackDamage = 7;
    private const double FleeChance = 0.5;

    public Ranger(string name) : base(name, MaxHitPointsValue, StartingMana)
    {
    }

    public string PerformAttack(ICombatant target)
    {
        target.TakeDamage(AttackDamage);
        return $"{Name} fires an arrow at {target.Name}. ({AttackDamage} damage)";
    }

    public bool AttemptFlee() => Random.Shared.NextDouble() < FleeChance;

    protected override string PerformTurnAction(ICombatant target) => PerformAttack(target);
}
