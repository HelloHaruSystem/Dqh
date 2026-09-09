using Dqh.Domain.Abilities;
using Dqh.Domain.Combatants;

namespace Dqh.Domain.Combatants.Adventurers;

/// <summary>Ranged attacker who can disengage from battle when things go wrong.</summary>
public sealed class Ranger : Adventurer, IAttacker, IFleeable
{
    private const int MaxHitPointsValue = 30;
    private const int StartingMana = 15;
    private const int SignatureMoveDamage = 16;
    private const int BasicAttackDamage = 8;
    private const double FleeChance = 0.5;

    public Ranger(string name) : base(name, MaxHitPointsValue, StartingMana)
    {
    }

    public override string UseSignatureMove(ICombatant target)
    {
        target.TakeDamage(SignatureMoveDamage);
        return $"{Name} looses a Piercing Shot at {target.Name}! ({SignatureMoveDamage} damage)";
    }

    public string PerformAttack(ICombatant target)
    {
        target.TakeDamage(BasicAttackDamage);
        return $"{Name} fires an arrow at {target.Name}. ({BasicAttackDamage} damage)";
    }

    public bool AttemptFlee() => Random.Shared.NextDouble() < FleeChance;
}
