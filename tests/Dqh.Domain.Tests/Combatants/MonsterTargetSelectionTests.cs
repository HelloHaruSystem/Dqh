using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Combatants.Monsters;
using Dqh.Domain.Strategies;

namespace Dqh.Domain.Tests.Combatants;

public class MonsterTargetSelectionTests
{
    [Fact]
    public void ChooseTarget_NoAvailableTargets_ReturnsNull()
    {
        var monster = Monster.Create(MonsterKind.Slime);

        var target = monster.ChooseTarget([]);

        Assert.Null(target);
    }

    [Fact]
    public void ChooseTarget_SwappingStrategy_ChangesResultWithoutTouchingMonster()
    {
        var warrior = new Warrior("Front");
        var mage = new Mage("Back");
        var availableTargets = new Adventurer[] { warrior, mage };

        var frontTargeting = Monster.Create(MonsterKind.Slime, new AlwaysFirstTargetStrategy());
        var backTargeting = Monster.Create(MonsterKind.Slime, new AlwaysLastTargetStrategy());

        Assert.Same(warrior, frontTargeting.ChooseTarget(availableTargets));
        Assert.Same(mage, backTargeting.ChooseTarget(availableTargets));
    }

    private sealed class AlwaysFirstTargetStrategy : ITargetSelectionStrategy
    {
        public Adventurer SelectTarget(IMonster attacker, IReadOnlyList<Adventurer> availableTargets) =>
            availableTargets[0];
    }

    private sealed class AlwaysLastTargetStrategy : ITargetSelectionStrategy
    {
        public Adventurer SelectTarget(IMonster attacker, IReadOnlyList<Adventurer> availableTargets) =>
            availableTargets[^1];
    }
}
