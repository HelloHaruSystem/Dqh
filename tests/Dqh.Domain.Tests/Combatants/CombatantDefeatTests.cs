using Dqh.Domain.Combatants.Monsters;

namespace Dqh.Domain.Tests.Combatants;

public class CombatantDefeatTests
{
    [Fact]
    public void TakeDamage_ExceedingCurrentHitPoints_ClampsAtZero_AndMarksDefeated()
    {
        var slime = Monster.Create(MonsterKind.Slime);

        slime.TakeDamage(9999);

        Assert.Equal(0, slime.CurrentHitPoints);
        Assert.True(slime.IsDefeated);
    }

    [Fact]
    public void Heal_ExceedingMaxHitPoints_ClampsAtMax()
    {
        var slime = Monster.Create(MonsterKind.Slime);
        slime.TakeDamage(5);

        slime.Heal(9999);

        Assert.Equal(slime.MaxHitPoints, slime.CurrentHitPoints);
    }
}
