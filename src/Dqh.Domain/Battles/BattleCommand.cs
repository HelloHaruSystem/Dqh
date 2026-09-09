using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Combatants.Monsters;
using Dqh.Domain.Magic;

namespace Dqh.Domain.Battles;

/// <summary>One adventurer's chosen action for a battle round.</summary>
public abstract record BattleCommand(Adventurer Actor);

/// <summary>Attacks a monster with a basic hit — every adventurer can do this.</summary>
public sealed record AttackCommand(Adventurer Actor, IMonster Target) : BattleCommand(Actor);

/// <summary>Casts a damage spell at a monster — only an <see cref="Abilities.ISpellcaster"/>.</summary>
public sealed record CastCommand(Adventurer Actor, DamageSpell Spell, IMonster Target) : BattleCommand(Actor);

/// <summary>Casts a healing spell on an ally — only an <see cref="Abilities.IHealer"/>.</summary>
public sealed record HealCommand(Adventurer Actor, HealingSpell Spell, Adventurer Target) : BattleCommand(Actor);

/// <summary>Braces for the next hit — only an <see cref="Abilities.IDefender"/>.</summary>
public sealed record GuardCommand(Adventurer Actor) : BattleCommand(Actor);

/// <summary>Attempts to flee the whole party from battle — only an <see cref="Abilities.IFleeable"/>.</summary>
public sealed record FleeCommand(Adventurer Actor) : BattleCommand(Actor);
