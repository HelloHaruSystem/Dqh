using Dqh.Domain.Combatants.Monsters;
using Dqh.Domain.Encounters;

namespace Dqh.Domain.Tests.Encounters;

public class EncounterTests
{
    [Fact]
    public void Constructor_WithNoMonsters_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Encounter([]));
    }

    [Fact]
    public void Constructor_WithGroupOfMonsters_KeepsAllOfThem()
    {
        var group = new IMonster[] { Monster.Create(MonsterKind.Slime), Monster.Create(MonsterKind.Slime), Monster.Create(MonsterKind.Dracky) };

        var encounter = new Encounter(group);

        Assert.Equal(3, encounter.Monsters.Count);
    }

    [Fact]
    public void IsResolved_StartsFalse()
    {
        var encounter = new Encounter(Monster.Create(MonsterKind.Ghost));

        Assert.False(encounter.IsResolved);
    }
}
