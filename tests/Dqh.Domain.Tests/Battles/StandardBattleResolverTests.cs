using Dqh.Domain.Battles;
using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Combatants.Monsters;
using Dqh.Domain.Strategies;

namespace Dqh.Domain.Tests.Battles;

public class StandardBattleResolverTests
{
    [Fact]
    public void ResolveRound_OrdersActorsBySpeedDescending()
    {
        var ranger = new Ranger("Ranger");
        var priest = new Priest("Priest");
        var party = new Domain.Party.Party(new RandomTargetStrategy());
        party.Register(priest);
        party.Register(ranger);
        var monster = Monster.Create(MonsterKind.Slime);
        var resolver = new StandardBattleResolver();

        var result = resolver.ResolveRound(party, [monster], [new AttackCommand(priest, monster), new AttackCommand(ranger, monster)]);

        Assert.Contains(ranger.Name, result.Log[0]);
    }

    [Fact]
    public void ResolveRound_AttackCommand_DamagesTarget()
    {
        var hero = new Hero("Hero");
        var party = new Domain.Party.Party(new RandomTargetStrategy());
        party.Register(hero);
        var monster = Monster.Create(MonsterKind.Slime);
        var startingHitPoints = monster.CurrentHitPoints;
        var resolver = new StandardBattleResolver();

        resolver.ResolveRound(party, [monster], [new AttackCommand(hero, monster)]);

        Assert.True(monster.CurrentHitPoints < startingHitPoints);
    }

    [Fact]
    public void ResolveRound_CastCommand_DamagesTarget()
    {
        var mage = new Mage("Mage");
        var party = new Domain.Party.Party(new RandomTargetStrategy());
        party.Register(mage);
        var monster = Monster.Create(MonsterKind.Ghost);
        var startingHitPoints = monster.CurrentHitPoints;
        var resolver = new StandardBattleResolver();

        resolver.ResolveRound(party, [monster], [new CastCommand(mage, mage.KnownSpells[0], monster)]);

        Assert.True(monster.CurrentHitPoints < startingHitPoints);
    }

    [Fact]
    public void ResolveRound_HealCommand_RestoresTargetHitPoints()
    {
        var priest = new Priest("Priest");
        var ally = new Warrior("Warrior");
        ally.TakeDamage(20);
        var party = new Domain.Party.Party(new RandomTargetStrategy());
        party.Register(priest);
        party.Register(ally);
        var monster = Monster.Create(MonsterKind.Slime);
        var beforeHeal = ally.CurrentHitPoints;
        var resolver = new StandardBattleResolver();

        resolver.ResolveRound(party, [monster], [new HealCommand(priest, priest.KnownHealingSpells[0], ally)]);

        Assert.True(ally.CurrentHitPoints > beforeHeal);
    }

    [Fact]
    public void ResolveRound_GuardCommand_SetsIsGuarding()
    {
        var warrior = new Warrior("Warrior");
        var party = new Domain.Party.Party(new RandomTargetStrategy());
        party.Register(warrior);
        var monster = Monster.Create(MonsterKind.Slime);
        var resolver = new StandardBattleResolver();

        resolver.ResolveRound(party, [monster], [new GuardCommand(warrior)]);

        Assert.True(warrior.IsGuarding);
    }

    [Fact]
    public void ResolveRound_FleeCommand_FirstLogMatchesOutcome()
    {
        // A failed attempt doesn't end the round early — the monster still
        // gets its turn — so only the first line (the attempt itself) is
        // deterministic here, not the log's total length.
        var ranger = new Ranger("Ranger");
        var party = new Domain.Party.Party(new RandomTargetStrategy());
        party.Register(ranger);
        var monster = Monster.Create(MonsterKind.Slime);
        var resolver = new StandardBattleResolver();

        var result = resolver.ResolveRound(party, [monster], [new FleeCommand(ranger)]);

        Assert.Equal(result.PartyFled, result.Log[0].Contains("retreat"));
    }

    [Fact]
    public void ResolveRound_DoesNotMutateGivenMonsterList()
    {
        var party = new Domain.Party.Party(new RandomTargetStrategy());
        party.Register(new Warrior("Warrior"));
        var monster = Monster.Create(MonsterKind.MetalSlime);
        IReadOnlyList<IMonster> monsters = [monster];
        var resolver = new StandardBattleResolver();

        resolver.ResolveRound(party, monsters, []);

        Assert.Single(monsters);
        Assert.Same(monster, monsters[0]);
    }
}
