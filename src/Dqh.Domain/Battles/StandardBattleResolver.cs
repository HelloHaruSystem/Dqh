using Dqh.Domain.Abilities;
using Dqh.Domain.Combatants.Monsters;
using Dqh.Domain.Party;

namespace Dqh.Domain.Battles;

/// <summary>Orders every living actor — party members with a command, plus every active monster — by Speed and resolves them in turn.</summary>
public sealed class StandardBattleResolver : IBattleResolver
{
    public BattleRoundResult ResolveRound(IParty party, IReadOnlyList<IMonster> monsters, IReadOnlyList<BattleCommand> partyCommands)
    {
        var log = new List<string>();
        var fled = new List<IMonster>();

        var actors = partyCommands
            .Select(command => new RoundActor(command.Actor.Speed, command, null))
            .Concat(monsters.Where(m => !m.IsDefeated).Select(m => new RoundActor(m.Speed, null, m)))
            .OrderByDescending(actor => actor.Speed);

        foreach (var actor in actors)
        {
            if (actor.Command is { } command)
            {
                if (command.Actor.IsDefeated) continue;

                if (command is FleeCommand flee)
                {
                    if (((IFleeable)flee.Actor).AttemptFlee())
                    {
                        log.Add($"{flee.Actor.Name} leads the party in a hasty retreat!");
                        return new BattleRoundResult(log, fled, PartyFled: true);
                    }

                    log.Add($"{flee.Actor.Name} tries to flee, but can't get away!");
                }
                else
                {
                    log.Add(ResolvePartyCommand(command));
                }
            }
            else
            {
                var monster = actor.Monster!;
                if (monster.IsDefeated || fled.Contains(monster)) continue;

                if (((IFleeable)monster).AttemptFlee())
                {
                    fled.Add(monster);
                    log.Add($"{monster.Name} flees from battle!");
                }
                else if (party.ChooseTarget(monster) is { } target)
                {
                    log.Add(((IAttacker)monster).PerformAttack(target));
                }
            }

            if (monsters.All(m => m.IsDefeated || fled.Contains(m))) break;
            if (party.Members.All(a => a.IsDefeated)) break;
        }

        return new BattleRoundResult(log, fled, PartyFled: false);
    }

    private static string ResolvePartyCommand(BattleCommand command) => command switch
    {
        AttackCommand attack => ((IAttacker)attack.Actor).PerformAttack(attack.Target),
        CastCommand cast => ((ISpellcaster)cast.Actor).Cast(cast.Spell, cast.Target),
        HealCommand heal => ((IHealer)heal.Actor).Heal(heal.Spell, heal.Target),
        GuardCommand guard => ((IDefender)guard.Actor).Guard(),
        _ => throw new ArgumentOutOfRangeException(nameof(command), command, "Unknown battle command."),
    };

    private readonly record struct RoundActor(int Speed, BattleCommand? Command, IMonster? Monster);
}
