using Dqh.Domain.Combatants.Monsters;
using Dqh.Domain.Party;

namespace Dqh.Domain.Battles;

/// <summary>Resolves one round of battle: every actor with a command this round acts once, fastest first.</summary>
public interface IBattleResolver
{
    /// <param name="party">The party in this battle.</param>
    /// <param name="monsters">The monsters currently active in this battle (already-fled ones dropped by the caller).</param>
    /// <param name="partyCommands">One command per living party member acting this round.</param>
    BattleRoundResult ResolveRound(IParty party, IReadOnlyList<IMonster> monsters, IReadOnlyList<BattleCommand> partyCommands);
}
