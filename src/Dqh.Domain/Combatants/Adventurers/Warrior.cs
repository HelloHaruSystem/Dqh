using Dqh.Domain.Abilities;
using Dqh.Domain.Combatants;

namespace Dqh.Domain.Combatants.Adventurers;

/// <summary>Frontline fighter who can also brace against incoming hits.</summary>
public sealed class Warrior : Adventurer, IAttacker, IDefender
{
    private const int MaxHitPointsValue = 40;
    private const int StartingMana = 10;
    private const int AttackDamage = 9;
    private const int SpeedValue = 8;

    public Warrior(string name) : base(name, MaxHitPointsValue, StartingMana, SpeedValue)
    {
    }

    public string PerformAttack(ICombatant target)
    {
        target.TakeDamage(AttackDamage);
        return $"{Name} swings their blade at {target.Name}. ({AttackDamage} damage)";
    }

    public string Guard()
    {
        SetGuarding();
        return $"{Name} raises their shield, bracing for the next hit.";
    }

    protected override string PerformTurnAction(ICombatant target) => PerformAttack(target);
}
