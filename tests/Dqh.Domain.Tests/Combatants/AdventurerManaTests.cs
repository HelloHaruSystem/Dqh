using Dqh.Domain.Combatants;

namespace Dqh.Domain.Tests.Combatants;

public class AdventurerManaTests
{
    [Fact]
    public void RestoreMana_ClampsAtMaximum()
    {
        var mage = new Mage("Test Mage");

        mage.RestoreMana(1000);

        Assert.Equal(100, mage.Mana);
    }

    [Fact]
    public void SpendMana_ClampsAtZero_NeverNegative()
    {
        var warrior = new Warrior("Test Warrior");

        warrior.SpendMana(1000);

        Assert.Equal(0, warrior.Mana);
    }
}
