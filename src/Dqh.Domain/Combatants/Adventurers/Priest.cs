using Dqh.Domain.Abilities;
using Dqh.Domain.Combatants;
using Dqh.Domain.Exceptions;
using Dqh.Domain.Magic;

namespace Dqh.Domain.Combatants.Adventurers;

/// <summary>Support caster with a small book of restorative spells, plus a weak mace attack when mana runs low.</summary>
public sealed class Priest : Adventurer, IAttacker, IHealer
{
    private const int MaxHitPointsValue = 26;
    private const int StartingMana = 50;
    private const int AttackDamage = 3;
    private const int SpeedValue = 6;

    private readonly List<HealingSpell> _knownSpells;

    public Priest(string name) : base(name, MaxHitPointsValue, StartingMana, SpeedValue)
    {
        _knownSpells =
        [
            SpellBook.GetHealingSpell(SpellId.Mend),
            SpellBook.GetHealingSpell(SpellId.GreaterMend),
        ];
    }

    public IReadOnlyList<HealingSpell> KnownHealingSpells => _knownSpells;

    public string PerformAttack(ICombatant target)
    {
        target.TakeDamage(AttackDamage);
        return $"{Name} taps {target.Name} with their mace. ({AttackDamage} damage)";
    }

    /// <exception cref="UnknownSpellException"><paramref name="spell"/> isn't in <see cref="KnownHealingSpells"/>.</exception>
    public string Heal(HealingSpell spell, ICombatant target)
    {
        if (!_knownSpells.Contains(spell))
            throw new UnknownSpellException($"{Name} does not know {spell.Name}.");

        if (!CanAfford(spell.ManaCost))
            return $"{Name} doesn't have enough mana to cast {spell.Name}.";

        SpendMana(spell.ManaCost);
        return spell.Apply(this, target);
    }

    protected override string PerformTurnAction(ICombatant target) => Heal(_knownSpells[^1], target);
}
