using Dqh.Domain.Abilities;
using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Combatants.Monsters;

namespace Dqh.Domain.Tests.Abilities;

public class AttackerAcrossHierarchiesTests
{
    [Fact]
    public void IAttacker_IsUsableIdenticallyForAdventurerAndMonster()
    {
        var target = Monster.Create(MonsterKind.Slime);
        IAttacker warrior = new Warrior("Test Warrior");
        IAttacker dracky = Monster.Create(MonsterKind.Dracky);

        var warriorResult = warrior.PerformAttack(target);
        var drackyResult = dracky.PerformAttack(target);

        Assert.False(string.IsNullOrWhiteSpace(warriorResult));
        Assert.False(string.IsNullOrWhiteSpace(drackyResult));
    }
}
