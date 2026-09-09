using Dqh.Domain.Abilities;
using Dqh.Domain.Combatants;
using Dqh.Domain.Magic;

namespace Dqh.Domain.Combatants.Adventurers;

/// <summary>Arcane damage-dealer with a small spellbook; the signature move casts its strongest known spell.</summary>
public sealed class Mage : Adventurer, ISpellcaster
{
    private const int MaxHitPointsValue = 22;
    private const int StartingMana = 60;

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

    /// <exception cref="ArgumentException"><paramref name="spell"/> isn't in <see cref="KnownSpells"/>.</exception>
    public string Cast(DamageSpell spell, ICombatant target)
    {
        if (!_knownSpells.Contains(spell))
            throw new ArgumentException($"{Name} does not know {spell.Name}.", nameof(spell));

        SpendMana(spell.ManaCost);
        return spell.Apply(this, target);
    }

    public override string UseSignatureMove(ICombatant target) => Cast(_knownSpells[^1], target);
}
