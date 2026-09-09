using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Combatants.Monsters;
using Dqh.Domain.Encounters;
using Dqh.Domain.Exceptions;
using Dqh.Domain.Party;
using Dqh.Domain.Strategies;

namespace Dqh.Domain.Tests.Party;

public class PartyTests
{
    private static readonly List<Encounter> ResolvedByNamedMethod = [];

    [Fact]
    public void Members_ReflectsRegisteredAdventurers()
    {
        var party = new Domain.Party.Party(new RandomTargetStrategy());
        var hero = new Hero("Test Hero");
        var priest = new Priest("Test Priest");
        party.Register(hero);
        party.Register(priest);

        Assert.Equal([hero, priest], party.Members);
    }

    [Fact]
    public void Register_SameAdventurerTwice_ThrowsAdventurerAlreadyRegisteredException()
    {
        var party = new Domain.Party.Party(new RandomTargetStrategy());
        var warrior = new Warrior("Test Warrior");
        party.Register(warrior);

        Assert.Throws<AdventurerAlreadyRegisteredException>(() => party.Register(warrior));
    }

    [Fact]
    public void ChooseTarget_WholePartyDefeated_ReturnsNull()
    {
        var party = new Domain.Party.Party(new RandomTargetStrategy());
        var warrior = new Warrior("Test Warrior");
        warrior.TakeDamage(1000);
        party.Register(warrior);
        var attacker = Monster.Create(MonsterKind.Slime);

        var target = party.ChooseTarget(attacker);

        Assert.Null(target);
    }

    [Fact]
    public void ChooseTarget_SwappingStrategy_ChangesResultWithoutTouchingParty()
    {
        var warrior = new Warrior("Front");
        var mage = new Mage("Back");
        var attacker = Monster.Create(MonsterKind.Slime);

        var alwaysFrontParty = new Domain.Party.Party(new AlwaysFirstTargetStrategy());
        alwaysFrontParty.Register(warrior);
        alwaysFrontParty.Register(mage);

        var alwaysBackParty = new Domain.Party.Party(new AlwaysLastTargetStrategy());
        alwaysBackParty.Register(warrior);
        alwaysBackParty.Register(mage);

        Assert.Same(warrior, alwaysFrontParty.ChooseTarget(attacker));
        Assert.Same(mage, alwaysBackParty.ChooseTarget(attacker));
    }

    [Fact]
    public void ResolveEncounter_InvokesCallback_WithNamedMethod()
    {
        var party = new Domain.Party.Party(new RandomTargetStrategy());
        var encounter = new Encounter(Monster.Create(MonsterKind.Slime));
        ResolvedByNamedMethod.Clear();

        party.ResolveEncounter(encounter, RecordResolution);

        Assert.True(encounter.IsResolved);
        Assert.Contains(encounter, ResolvedByNamedMethod);
    }

    [Fact]
    public void ResolveEncounter_InvokesCallback_WithLambda()
    {
        var party = new Domain.Party.Party(new RandomTargetStrategy());
        var encounter = new Encounter(Monster.Create(MonsterKind.Slime));
        var wasCalled = false;

        party.ResolveEncounter(encounter, resolved => wasCalled = resolved.IsResolved);

        Assert.True(wasCalled);
    }

    [Fact]
    public void FindAvailableHealer_ReturnsLivingHealer()
    {
        var party = new Domain.Party.Party(new RandomTargetStrategy());
        var warrior = new Warrior("Test Warrior");
        var priest = new Priest("Test Priest");
        party.Register(warrior);
        party.Register(priest);

        var healer = party.FindAvailableHealer();

        Assert.Same(priest, healer);
    }

    [Fact]
    public void FindFirstUnresolvedEncounter_SkipsResolvedOnes()
    {
        var party = new Domain.Party.Party(new RandomTargetStrategy());
        var resolved = new Encounter(Monster.Create(MonsterKind.Slime));
        var unresolved = new Encounter(Monster.Create(MonsterKind.Ghost));
        party.Report(resolved);
        party.Report(unresolved);
        party.ResolveEncounter(resolved, _ => { });

        var found = party.FindFirstUnresolvedEncounter();

        Assert.Same(unresolved, found);
    }

    private static void RecordResolution(Encounter encounter) => ResolvedByNamedMethod.Add(encounter);

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
