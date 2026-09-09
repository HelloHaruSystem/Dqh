using Dqh.Domain.Combatants;
using Dqh.Domain.Combatants.Monsters;

namespace Dqh.Domain.Magic;

/// <summary>An offensive spell. Deals bonus damage to a monster weak to its element.</summary>
public sealed class DamageSpell : ISpell
{
    private const double WeaknessMultiplier = 2.0;

    private readonly int _power;
    private readonly string _descriptionTemplate;

    public string Name { get; }
    public int ManaCost { get; }
    public ElementType Element { get; }

    /// <param name="descriptionTemplate">Flavor text with {0}=caster name, {1}=target name.</param>
    public DamageSpell(string name, int manaCost, int power, ElementType element, string descriptionTemplate)
    {
        SpellGuard.EnsureValid(name, manaCost);
        Name = name;
        ManaCost = manaCost;
        _power = power;
        Element = element;
        _descriptionTemplate = descriptionTemplate;
    }

    public string Apply(ICombatant caster, ICombatant target)
    {
        var isWeak = target is IMonster monster && monster.IsWeakTo(Element);
        var damage = isWeak ? (int)(_power * WeaknessMultiplier) : _power;
        target.TakeDamage(damage);

        var description = string.Format(_descriptionTemplate, caster.Name, target.Name);
        var suffix = isWeak ? " A critical weakness is struck!" : "";
        return $"{description} ({damage} damage){suffix}";
    }
}
