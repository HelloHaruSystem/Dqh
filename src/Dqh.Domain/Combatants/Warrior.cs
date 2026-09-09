using Dqh.Domain.Abilities;

namespace Dqh.Domain.Combatants;

/// <summary>Frontline fighter: strong melee damage, can brace against incoming hits.</summary>
public sealed class Warrior : Adventurer, IAttacker, IDefender
{
    private const int MaxHitPointsValue = 40;
    private const int StartingMana = 10;
    private const int SignatureMoveDamage = 14;
    private const int BasicAttackDamage = 7;

    public Warrior(string name) : base(name, MaxHitPointsValue, StartingMana)
    {
    }

    public override string UseSignatureMove(ICombatant target)
    {
        target.TakeDamage(SignatureMoveDamage);
        return $"{Name} unleashes a Cleaving Strike on {target.Name}! ({SignatureMoveDamage} damage)";
    }

    public string PerformAttack(ICombatant target)
    {
        target.TakeDamage(BasicAttackDamage);
        return $"{Name} swings their blade at {target.Name}. ({BasicAttackDamage} damage)";
    }

    public string Guard() => $"{Name} raises their shield, bracing for the next hit.";
}
