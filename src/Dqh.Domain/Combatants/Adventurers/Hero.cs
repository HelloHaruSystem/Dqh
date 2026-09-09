using Dqh.Domain.Abilities;
using Dqh.Domain.Combatants;

namespace Dqh.Domain.Combatants.Adventurers;

/// <summary>The party's protagonist: a capable fighter in their own right, distinct from a plain Warrior.</summary>
public sealed class Hero : Adventurer, IAttacker
{
    private const int MaxHitPointsValue = 45;
    private const int StartingMana = 15;
    private const int SignatureMoveDamage = 16;
    private const int BasicAttackDamage = 8;

    public Hero(string name) : base(name, MaxHitPointsValue, StartingMana)
    {
    }

    public override string UseSignatureMove(ICombatant target)
    {
        target.TakeDamage(SignatureMoveDamage);
        return $"{Name} channels their storied blade into a Radiant Slash on {target.Name}! ({SignatureMoveDamage} damage)";
    }

    public string PerformAttack(ICombatant target)
    {
        target.TakeDamage(BasicAttackDamage);
        return $"{Name} strikes at {target.Name}. ({BasicAttackDamage} damage)";
    }
}
