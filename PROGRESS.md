# Progress log

Running record of work across sessions, kept in the repo so any session can pick up
context without relying on chat history. See [`docs/domain-model.md`](docs/domain-model.md)
for the design; this file tracks *build status* against that design.

The UML lives as a Mermaid diagram embedded in `docs/domain-model.md`. Keep this
discipline: refresh the diagram whenever the domain design changes, not just once
at the end.

## Kernekrav checklist (DQH names)

| Kernekrav | DQH concept | Status |
|---|---|---|
| a) abstract base + polymorphism + encapsulation | `Adventurer` (abstract) + `Hero`/`Warrior`/`Mage`/`Priest`/`Ranger` | **Done** |
| b) ≥2 self-made interfaces | `IAttacker`, `ISpellcaster`, `IHealer`, `IDefender`, `IFleeable` | **Done** |
| c) manager class + collections | `Party` / `IParty` (roster + encounter log) | **Done** |
| d) generic search method | `DomainToolbox.FindFirst<T>` (used on both the roster and the encounter log) | **Done** |
| e) ≥2 custom exceptions | `UnknownSpellException`, `AdventurerAlreadyRegisteredException` | **Done** |
| f) callback on resolution | `Party.ResolveEncounter(Encounter, Action<Encounter>)` | **Done** |
| g) UML before coding | `docs/domain-model.md` (Mermaid) | **Done** — matches current code |
| h) dependency inversion via injected strategy | `ITargetSelectionStrategy` → `Party` ctor | **Done** |
| i) documentation & git history | README, XML docs, incremental commits | In progress (README covers build/run; written justification for h) still missing) |

All of kernekrav a–h are now built. Remaining before hand-in: items/i)'s written
justification, and whichever Day-2 alternative track gets picked (see below).

## What's actually built (`src/Dqh.Domain`)

- **Combat core**: `ICombatant` → `Adventurer` (abstract; encapsulated `Mana`
  0–100, HP via composed `HitPointTrack`) → `Hero`, `Warrior`, `Mage`, `Priest`,
  `Ranger` (sealed). `Adventurer.TakeTurn(target)` is a template method — guards
  `IsDefeated` (returns a message, doesn't throw — see exceptions note below),
  then calls the abstract `PerformTurnAction`, which each subclass overrides.
  This *is* the polymorphism proof for kernekrav a — not a separate "signature
  move" layered on top of a basic attack, since turn-based DQ doesn't really have
  that split (each class has one action type per turn, chosen from a menu).
- **Every adventurer can attack; some also have one special ability**:
  `IAttacker` is implemented by all five (matches the DQ battle menu — Attack is
  always available). `ISpellcaster` (`Mage`), `IHealer` (`Priest`), `IDefender`
  (`Warrior`), `IFleeable` (`Ranger`) are each exclusive to one — kernekrav b's
  "not all heroes have this" interfaces. `IAttacker`/`IFleeable` are also
  implemented by the unrelated `Monster` class (cross-hierarchy proof).
- **Monsters, data-driven, not one class per kind**: `MonsterKind` → 
  `MonsterDefinition` (stats/weaknesses/flavor text/`AttackWeight`+`FleeWeight`) →
  `MonsterBestiary` (internal lookup table) → single `Monster` class, built via
  `Monster.Create(MonsterKind)`. Roster: `Slime`/`Dracky`/`Ghost` (one small
  starting area, always attack) + `MetalSlime` (mostly flees — `AttackWeight`
  10/`FleeWeight` 90 — matching its real reputation; `Monster.AttemptFlee()`
  rolls against these weights). This mirrors how the original games actually
  stored monster behavior (a per-species weight table), not a strategy class per
  monster.
- **Spells, also data-driven**: `ISpell` → `DamageSpell`/`HealingSpell` (no
  shared base — validation shared via internal `SpellGuard`) → `SpellId` →
  `SpellBook`. `ElementType` (`Physical`/`Fire`/`Ice`/`Wind`/`Explosion`) drives
  the weakness mechanic: a `DamageSpell` deals bonus damage to an `IMonster` weak
  to its element.
- **`Encounter`**: a monster *group* (`IReadOnlyList<IMonster>`, one or more) +
  `IsResolved`. No adventurer reference on it — the whole living party faces an
  encounter together, there's no "one hero is dispatched to handle it."
- **`IParty`/`Party`**: `Register`/`Report` (roster + encounter log, kernekrav c),
  `ChooseTarget(attacker)` (uses the injected `ITargetSelectionStrategy` —
  kernekrav h — to pick which living member a monster's attack lands on; `null`
  if the whole party is defeated), `ResolveEncounter` (the callback, kernekrav f),
  `FindAvailableHealer()`/`FindFirstUnresolvedEncounter()` (the generic search,
  kernekrav d, over the roster and the encounter log respectively).
- **`ITargetSelectionStrategy`/`RandomTargetStrategy`** (kernekrav h): injected
  into `Party`'s constructor. Targets a uniformly random living party member —
  matches how DQ's actual targeting works (mostly random/formation-weighted, not
  a computed "best target").
- **Exceptions are for caller bugs, not expected game states.** A party wipe,
  low mana, or a defeated adventurer trying to act are normal outcomes of play —
  modeled as a `null` return / a descriptive message, not a throw.
  `UnknownSpellException` (casting a spell you don't know) and
  `AdventurerAlreadyRegisteredException` (registering the same adventurer twice)
  only happen from an actual bug in the calling code, which is what an exception
  should mean — kept it to exactly those two rather than force-fitting the
  brief's suggested "busy hero"/"no suitable hero" examples onto DQ.
- All enums use explicit numeric values (1-based) rather than implicit ordinals.
- `Combatants/` split into `Combatants/Adventurers/` and `Combatants/Monsters/`
  (namespaces matching folders) once the flat folder grew past ~10 files.
- 25 xUnit tests in `tests/Dqh.Domain.Tests`, all passing.

## Designed but not yet built

- Items: `IItem`, `IUsable`, `IEquippable`, `HealingPotion`, `Sword`
- `IEncounterGenerator` (factory for random encounters on overworld tiles)
- `IBattleResolver` (swappable battle resolution, using `Party.ChooseTarget` and
  `Monster.AttemptFlee`/`PerformAttack` to actually run a fight turn by turn)
- Wiring `Encounter`/`Party`/battle resolution into the `Dqh.Game` raylib loop
- Written justification for kernekrav h (why loose coupling makes the targeting
  policy easy to change later) — a few sentences, per kernekrav i
- Day-2 alternative-track choice (kernekrav's thread race-condition demo vs. one
  of the three alternatives — swap strategy, save/load, or an extra Adventurer
  tier with its own state) — **not decided yet**

## Session log

### 2026-09-09 — Session 1

Built the combat core (`ICombatant`/`Adventurer`+subclasses, data-driven
monsters/spells), the raylib overworld proof-of-concept in `Dqh.Game` with a
headless mode and a resizable window, and switched the UML from a hand-placed
draw.io file to Mermaid. Full detail was here before being trimmed for length —
see git history for this file if needed. Ended with: combat core done, `Encounter`/
`Party`/strategy/exceptions/generics/callback still to build.

### 2026-09-09 — Session 2 (`party_and_encounters` branch)

Built kernekrav c/d/e/f/h — see "What's actually built" above for the final
shape. Getting there took a fair amount of back-and-forth on what's actually
DQ-authentic vs. a direct translation of the Heltevagten brief's dispatch-center
framing (now written down in `CLAUDE.md` so it doesn't need relearning):

- First pass modeled `Encounter` with the brief's own fields (description,
  location, a severity rating, a single adventurer "responder"/"champion"
  assigned to it) — rejected step by step: DQ has no player-facing severity
  concept, and DQ battles are the whole living party fighting together, not one
  hero dispatched to handle an incident. `Encounter` ended up as just a monster
  group + resolved flag.
- Tried `ILeaderSelectionStrategy` (who leads the party) for kernekrav h's
  injected strategy — dropped as abstraction with no real second
  implementation worth swapping to. Landed on `ITargetSelectionStrategy`
  (which party member a monster attacks) instead, since that has genuinely
  differentiated concrete strategies worth swapping between.
- Tried modeling monster flee behavior as its own strategy interface
  (`IFleeDecisionStrategy`) — dropped in favor of plain weighted data
  (`AttackWeight`/`FleeWeight` on `MonsterDefinition`) after checking how the
  original games actually did it: a per-species weight table, not polymorphism.
- `InsufficientManaException`/an "unavailable adventurer" exception were dropped
  once we drew the line clearly: exceptions are for caller-code bugs, not
  expected/recoverable game states (low mana, party wipes) — those became
  checked returns instead. The two exceptions that survived (`UnknownSpellException`,
  `AdventurerAlreadyRegisteredException`) are both genuine "this should never
  happen through normal play" caller-misuse cases.
- Dropped the separate "signature move vs. basic attack" split (`UseSignatureMove`
  as an extra strong move on top of a weaker `PerformAttack`) once we noted
  turn-based DQ doesn't have that — one action type per turn per class. Collapsed
  into `TakeTurn`/`PerformTurnAction`, and gave every adventurer `IAttacker` too
  (matching the DQ menu: Attack always available, plus one class-specific option).

Next session:
1. Items (`IItem`/`IUsable`/`IEquippable`), `IEncounterGenerator`, `IBattleResolver`
   (using the pieces already built: `Party.ChooseTarget`, `Monster.AttemptFlee`).
2. Wire `Encounter`/`Party`/battle resolution into the `Dqh.Game` raylib loop so
   stepping into an encounter tile actually triggers a fight.
3. Write the kernekrav h justification paragraph, pick the Day-2 alternative track.
4. Keep `docs/domain-model.md` updated as each of the above lands.
