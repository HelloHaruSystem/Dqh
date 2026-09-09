using Dqh.Domain.Abilities;
using Dqh.Domain.Combatants;
using Dqh.Domain.Magic;

namespace Dqh.Domain.Combatants.Adventurers;

/// <summary>Support caster with a small book of restorative spells; the signature move casts its strongest.</summary>
public sealed class Priest : Adventurer, IHealer
{
    private const int MaxHitPointsValue = 26;
    private const int StartingMana = 50;

    private readonly List<HealingSpell> _knownSpells;

    public Priest(string name) : base(name, MaxHitPointsValue, StartingMana)
    {
        _knownSpells =
        [
            SpellBook.GetHealingSpell(SpellId.Mend),
            SpellBook.GetHealingSpell(SpellId.GreaterMend),
        ];
    }

    public IReadOnlyList<HealingSpell> KnownHealingSpells => _knownSpells;

    /// <exception cref="ArgumentException"><paramref name="spell"/> isn't in <see cref="KnownHealingSpells"/>.</exception>
    public string Heal(HealingSpell spell, ICombatant target)
    {
        if (!_knownSpells.Contains(spell))
            throw new ArgumentException($"{Name} does not know {spell.Name}.", nameof(spell));

        SpendMana(spell.ManaCost);
        return spell.Apply(this, target);
    }

    public override string UseSignatureMove(ICombatant target) => Heal(_knownSpells[^1], target);
}
