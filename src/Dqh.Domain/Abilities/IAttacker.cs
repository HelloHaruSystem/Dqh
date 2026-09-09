using Dqh.Domain.Combatants;

namespace Dqh.Domain.Abilities;

/// <summary>Can perform a basic physical attack. Crosses both the Adventurer hierarchy and standalone monsters.</summary>
public interface IAttacker
{
    /// <returns>Flavor text describing the attack.</returns>
    string PerformAttack(ICombatant target);
}
