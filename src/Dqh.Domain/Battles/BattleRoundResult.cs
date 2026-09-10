using Dqh.Domain.Combatants.Monsters;

namespace Dqh.Domain.Battles;

/// <summary>What happened when a round of battle was resolved.</summary>
/// <param name="Log">One flavor line per action taken, in the order actors acted.</param>
/// <param name="FledMonsters">Monsters that escaped this round — the caller is responsible for dropping them from play.</param>
/// <param name="PartyFled">True if the whole party escaped this round, ending the battle immediately.</param>
public sealed record BattleRoundResult(IReadOnlyList<string> Log, IReadOnlyList<IMonster> FledMonsters, bool PartyFled);
