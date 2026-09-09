using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Combatants.Monsters;

namespace Dqh.Domain.Tests.Combatants;

public class TakeTurnPolymorphismTests
{
    public static IEnumerable<object[]> Adventurers()
    {
        yield return new object[] { new Hero("Hero") };
        yield return new object[] { new Warrior("Warrior") };
        yield return new object[] { new Mage("Mage") };
        yield return new object[] { new Priest("Priest") };
        yield return new object[] { new Ranger("Ranger") };
    }

    [Theory]
    [MemberData(nameof(Adventurers))]
    public void TakeTurn_ReturnsNonEmptyDescription(Adventurer adventurer)
    {
        var target = Monster.Create(MonsterKind.Slime);

        var description = adventurer.TakeTurn(target);

        Assert.False(string.IsNullOrWhiteSpace(description));
    }

    [Fact]
    public void EachConcreteAdventurer_OverridesTurnActionDifferently()
    {
        var target = Monster.Create(MonsterKind.Slime);
        var ally = new Warrior("Ally");

        var descriptions = new HashSet<string>
        {
            new Hero("Hero").TakeTurn(target),
            new Warrior("Warrior").TakeTurn(target),
            new Mage("Mage").TakeTurn(target),
            new Priest("Priest").TakeTurn(ally),
            new Ranger("Ranger").TakeTurn(target),
        };

        Assert.Equal(5, descriptions.Count);
    }

    [Fact]
    public void TakeTurn_OnDefeatedAdventurer_ReturnsMessageWithoutActing()
    {
        var warrior = new Warrior("Fallen Warrior");
        warrior.TakeDamage(1000);
        var target = Monster.Create(MonsterKind.Slime);
        var startingHitPoints = target.CurrentHitPoints;

        var description = warrior.TakeTurn(target);

        Assert.Contains("defeated", description);
        Assert.Equal(startingHitPoints, target.CurrentHitPoints);
    }
}
