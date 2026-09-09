using Dqh.Domain.Combatants.Adventurers;

namespace Dqh.Domain.Tests.Combatants;

public class AdventurerFullyRestoreTests
{
    [Fact]
    public void FullyRestore_HealsToMaxHitPointsAndMana()
    {
        var mage = new Mage("Test Mage");
        mage.TakeDamage(1000);
        mage.SpendMana(1000);

        mage.FullyRestore();

        Assert.Equal(mage.MaxHitPoints, mage.CurrentHitPoints);
        Assert.Equal(100, mage.Mana);
    }
}
