using Dqh.Domain.Combatants.Adventurers;

namespace Dqh.Domain.Tests.Combatants;

public class GuardTests
{
    [Fact]
    public void TakeDamage_WhileGuarding_HalvesDamage()
    {
        var warrior = new Warrior("Warrior");
        warrior.Guard();

        warrior.TakeDamage(10);

        Assert.Equal(warrior.MaxHitPoints - 5, warrior.CurrentHitPoints);
    }

    [Fact]
    public void ResetGuard_ClearsGuardingState()
    {
        var warrior = new Warrior("Warrior");
        warrior.Guard();

        warrior.ResetGuard();
        warrior.TakeDamage(10);

        Assert.False(warrior.IsGuarding);
        Assert.Equal(warrior.MaxHitPoints - 10, warrior.CurrentHitPoints);
    }
}
