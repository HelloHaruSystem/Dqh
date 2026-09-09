using Dqh.Domain.Abilities;
using Dqh.Domain.Combatants;

namespace Dqh.Domain.Combatants.Adventurers;

/// <summary>The party's protagonist.</summary>
public sealed class Hero : Adventurer, IAttacker
{
    private const int MaxHitPointsValue = 45;
    private const int StartingMana = 15;
    private const int AttackDamage = 8;

    public Hero(string name) : base(name, MaxHitPointsValue, StartingMana)
    {
    }

    public string PerformAttack(ICombatant target)
    {
        target.TakeDamage(AttackDamage);
        return $"{Name} strikes at {target.Name}. ({AttackDamage} damage)";
    }

    protected override string PerformTurnAction(ICombatant target) => PerformAttack(target);
}
