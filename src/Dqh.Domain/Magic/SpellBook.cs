namespace Dqh.Domain.Magic;

/// <summary>
/// Central catalog of named spells, keyed by <see cref="SpellId"/> — the spell
/// equivalent of <see cref="Combatants.MonsterBestiary"/>. Adventurer types look
/// spells up here instead of constructing them inline.
/// </summary>
internal static class SpellBook
{
    private static readonly Dictionary<SpellId, DamageSpell> DamageSpells = new()
    {
        [SpellId.Ember] = new DamageSpell(
            "Ember", manaCost: 6, power: 8, element: ElementType.Fire,
            descriptionTemplate: "{0} channels a burst of flame at {1}"),
        [SpellId.Inferno] = new DamageSpell(
            "Inferno", manaCost: 16, power: 20, element: ElementType.Fire,
            descriptionTemplate: "{0} conjures a raging inferno around {1}"),
    };

    private static readonly Dictionary<SpellId, HealingSpell> HealingSpells = new()
    {
        [SpellId.Mend] = new HealingSpell(
            "Mend", manaCost: 5, power: 12,
            descriptionTemplate: "{0} weaves a mending light over {1}"),
        [SpellId.GreaterMend] = new HealingSpell(
            "Greater Mend", manaCost: 14, power: 30,
            descriptionTemplate: "{0} channels radiant light to restore {1}"),
    };

    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> has no damage spell entry.</exception>
    public static DamageSpell GetDamageSpell(SpellId id) =>
        DamageSpells.TryGetValue(id, out var spell)
            ? spell
            : throw new ArgumentOutOfRangeException(nameof(id), id, "No damage spell entry for this id.");

    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> has no healing spell entry.</exception>
    public static HealingSpell GetHealingSpell(SpellId id) =>
        HealingSpells.TryGetValue(id, out var spell)
            ? spell
            : throw new ArgumentOutOfRangeException(nameof(id), id, "No healing spell entry for this id.");
}
