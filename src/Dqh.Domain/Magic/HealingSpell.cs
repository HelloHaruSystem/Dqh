using Dqh.Domain.Combatants;

namespace Dqh.Domain.Magic;

/// <summary>A restorative spell that heals a single target.</summary>
public sealed class HealingSpell : ISpell
{
    private readonly int _power;
    private readonly string _descriptionTemplate;

    public string Name { get; }
    public int ManaCost { get; }

    /// <param name="descriptionTemplate">Flavor text with {0}=caster name, {1}=target name.</param>
    public HealingSpell(string name, int manaCost, int power, string descriptionTemplate)
    {
        SpellGuard.EnsureValid(name, manaCost);
        Name = name;
        ManaCost = manaCost;
        _power = power;
        _descriptionTemplate = descriptionTemplate;
    }

    public string Apply(ICombatant caster, ICombatant target)
    {
        target.Heal(_power);
        var description = string.Format(_descriptionTemplate, caster.Name, target.Name);
        return $"{description} (+{_power} HP)";
    }
}
