using Dqh.Domain.Combatants;
using Dqh.Domain.Magic;

namespace Dqh.Domain.Tests.Magic;

public class DamageSpellWeaknessTests
{
    [Fact]
    public void Apply_DealsBonusDamage_WhenTargetIsWeakToElement()
    {
        var caster = new Mage("Test Mage");
        var weakTarget = Monster.Create(MonsterKind.Slime); // weak to Fire
        var resistantTarget = Monster.Create(MonsterKind.Dracky); // weak to Ice, not Fire
        var emberSpell = caster.KnownSpells[0]; // Ember, Fire element

        emberSpell.Apply(caster, weakTarget);
        emberSpell.Apply(caster, resistantTarget);

        var damageToWeakTarget = weakTarget.MaxHitPoints - weakTarget.CurrentHitPoints;
        var damageToResistantTarget = resistantTarget.MaxHitPoints - resistantTarget.CurrentHitPoints;

        Assert.True(damageToWeakTarget > damageToResistantTarget);
    }

    [Fact]
    public void Cast_SpellNotInKnownList_ThrowsArgumentException()
    {
        var mage = new Mage("Test Mage");
        var target = Monster.Create(MonsterKind.Slime);
        var unknownSpell = new DamageSpell(
            "Improvised Bolt", manaCost: 1, power: 1, element: ElementType.Physical,
            descriptionTemplate: "{0} improvises against {1}");

        Assert.Throws<ArgumentException>(() => mage.Cast(unknownSpell, target));
    }
}
