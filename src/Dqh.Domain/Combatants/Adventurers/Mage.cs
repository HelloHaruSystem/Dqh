using Dqh.Domain.Abilities;
using Dqh.Domain.Combatants;
using Dqh.Domain.Exceptions;
using Dqh.Domain.Magic;

namespace Dqh.Domain.Combatants.Adventurers;

/// <summary>Arcane damage-dealer with a small spellbook, plus a weak staff attack when mana runs low.</summary>
public sealed class Mage : Adventurer, IAttacker, ISpellcaster
{
    private const int MaxHitPointsValue = 22;
    private const int StartingMana = 60;
    private const int AttackDamage = 3;

    private readonly List<DamageSpell> _knownSpells;

    public Mage(string name) : base(name, MaxHitPointsValue, StartingMana)
    {
        _knownSpells =
        [
            SpellBook.GetDamageSpell(SpellId.Ember),
            SpellBook.GetDamageSpell(SpellId.Inferno),
        ];
    }

    public IReadOnlyList<DamageSpell> KnownSpells => _knownSpells;

    public string PerformAttack(ICombatant target)
    {
        target.TakeDamage(AttackDamage);
        return $"{Name} jabs {target.Name} with their staff. ({AttackDamage} damage)";
    }

    /// <exception cref="UnknownSpellException"><paramref name="spell"/> isn't in <see cref="KnownSpells"/>.</exception>
    public string Cast(DamageSpell spell, ICombatant target)
    {
        if (!_knownSpells.Contains(spell))
            throw new UnknownSpellException($"{Name} does not know {spell.Name}.");

        if (!CanAfford(spell.ManaCost))
            return $"{Name} doesn't have enough mana to cast {spell.Name}.";

        SpendMana(spell.ManaCost);
        return spell.Apply(this, target);
    }

    protected override string PerformTurnAction(ICombatant target) => Cast(_knownSpells[^1], target);
}
