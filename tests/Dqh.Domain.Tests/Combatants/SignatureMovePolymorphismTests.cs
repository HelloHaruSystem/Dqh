using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Combatants.Monsters;

namespace Dqh.Domain.Tests.Combatants;

public class SignatureMovePolymorphismTests
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
    public void UseSignatureMove_ReturnsNonEmptyDescription(Adventurer adventurer)
    {
        var target = Monster.Create(MonsterKind.Slime);

        var description = adventurer.UseSignatureMove(target);

        Assert.False(string.IsNullOrWhiteSpace(description));
    }

    [Fact]
    public void EachConcreteAdventurer_OverridesSignatureMoveDifferently()
    {
        var target = Monster.Create(MonsterKind.Slime);
        var ally = new Warrior("Ally");

        var descriptions = new HashSet<string>
        {
            new Hero("Hero").UseSignatureMove(target),
            new Warrior("Warrior").UseSignatureMove(target),
            new Mage("Mage").UseSignatureMove(target),
            new Priest("Priest").UseSignatureMove(ally),
            new Ranger("Ranger").UseSignatureMove(target),
        };

        Assert.Equal(5, descriptions.Count);
    }
}
