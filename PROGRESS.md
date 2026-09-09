# Progress log

Running record of work across sessions, kept in the repo so any session can pick up
context without relying on chat history. See [`docs/domain-model.md`](docs/domain-model.md)
for the design; this file tracks *build status* against that design.

**⚠ `docs/domain-model.drawio`/`.md` are stale relative to the code below** — the
combat core evolved significantly during implementation (see "Design changes since
the UML" below). Refresh both at the start of the next session, before adding
`Encounter`/`Party` (kernekrav g requires the diagram to match the final code, and
it's much cheaper to keep it current incrementally than to reconstruct it later).

## Kernekrav checklist (DQH names)

| Kernekrav | DQH concept | Status |
|---|---|---|
| a) abstract base + polymorphism + encapsulation | `Adventurer` (abstract) + `Hero`/`Warrior`/`Mage`/`Priest`/`Ranger` | **Done** |
| b) ≥2 self-made interfaces | `IAttacker`, `ISpellcaster`, `IHealer`, `IDefender`, `IFleeable` | **Done** |
| c) manager class + collections | `Party` / `IParty` | Not started |
| d) generic search method | `DomainToolbox.FindFirst<T>` | Not started |
| e) ≥2 custom exceptions | `AdventurerUnavailableException`, `NoSuitableAdventurerFoundException`, `InsufficientManaException` | Not started (see note below) |
| f) callback on resolution | `Party.ResolveEncounter(Encounter, Action<Encounter>)` | Not started |
| g) UML before coding | `docs/domain-model.drawio` / `.md` | Done, but **stale** — refresh next session |
| h) dependency inversion via injected strategy | `IChampionSelectionStrategy` → `Party` ctor | Not started |
| i) documentation & git history | README, XML docs, incremental commits | In progress (XML docs done for everything built so far; README still missing) |

## What's actually built (`src/Dqh.Domain`)

- **Combat core**: `ICombatant` (interface) → `Adventurer` (abstract; encapsulated
  `Mana` 0–100, HP via composed `HitPointTrack`, abstract `UseSignatureMove`) →
  `Hero`, `Warrior`, `Mage`, `Priest`, `Ranger` (sealed). This is the DQ3-style
  lineup (Hero + the classic Warrior/Mage/Priest vocations, plus Ranger for
  `IFleeable`), not the originally-planned generic Warrior/Mage/Healer/Ranger.
- **Monsters, data-driven, not one class per kind**: `MonsterKind` (enum) →
  `MonsterDefinition` (stats/weaknesses/flavor text) → `MonsterBestiary` (internal
  lookup table) → single `Monster` class (`IMonster` + `IAttacker`), built via
  `Monster.Create(MonsterKind)`. Current roster is a DQ1-style single starting area:
  `Slime` (weak to Fire), `Dracky` (weak to Ice), `Ghost` (weak to Wind) — deliberately
  *not* Goblin/Dragon, which don't fit "one small starting area."
- **Spells, also data-driven**: `ISpell` → `DamageSpell`/`HealingSpell` (sealed, no
  shared base — validation shared via internal `SpellGuard`, not inheritance) →
  `SpellId` (enum) → `SpellBook` (internal catalog, mirrors `MonsterBestiary`).
  `ElementType` (`Physical`/`Fire`/`Ice`/`Wind`/`Explosion`) drives the weakness
  mechanic: a `DamageSpell` deals bonus damage to an `IMonster` weak to its element.
- **Ability interfaces**: `IAttacker` (both `Hero`/`Warrior`/`Ranger` *and* `Monster`
  implement it — the "independent of the inheritance hierarchy" proof), `ISpellcaster`
  (`Mage`), `IHealer` (`Priest`), `IDefender` (`Warrior` only), `IFleeable` (`Ranger`
  only).
- All enums use explicit numeric values (1-based) rather than implicit ordinals, so
  reordering/inserting members later can't silently renumber existing ones.
- 13 xUnit tests in `tests/Dqh.Domain.Tests` covering mana clamping, signature-move
  polymorphism, HP clamping/defeat, elemental weakness bonus damage, and `IAttacker`
  crossing the Adventurer/Monster split. All passing.

## Design changes since the original UML (why the diagram needs a refresh)

- No `Monster` abstract class / no `Slime`/`Goblin`/`Dragon` subclasses — replaced by
  the single data-driven `Monster` class above. Decided against a shared
  `Adventurer`/`Monster` base class too (`Character`): the two only share `Name` + HP
  bookkeeping, which is thin enough for composition (`HitPointTrack`); they diverge on
  everything that actually matters (mana/spellbook vs. AI/rewards), so a shared base
  would either be near-empty or accumulate half-used properties.
- No `Spell` abstract base — `ISpell` interface instead, since `DamageSpell`/
  `HealingSpell` share almost no logic beyond name/cost validation.
- `IPhysicalAttacker` → merged into `IAttacker`, shared by adventurers and monsters.
- Roster is `Hero`/`Warrior`/`Mage`/`Priest`/`Ranger`, not `Warrior`/`Mage`/`Healer`/
  `Ranger` — "Healer" isn't the DQ term (it's `Priest`), and DQ's actual iconic
  lineup always has a `Hero` leading vocation-classed companions.
- Added the whole `Magic` namespace (`ElementType`, `ISpell`, `DamageSpell`,
  `HealingSpell`, `SpellId`, `SpellBook`, `SpellGuard`) and `IMonster`/`MonsterKind`/
  `MonsterDefinition`/`MonsterBestiary` — none of this existed in the first UML pass.

## Designed but not yet built

- `Encounter`, `IParty`/`Party`, `IChampionSelectionStrategy` (+ `FirstAvailableChampionStrategy`)
- The two/three custom exceptions — note `Adventurer.SpendMana` currently just clamps
  mana at 0 instead of throwing `InsufficientManaException`; that needs wiring in
  once the exception exists
- `DomainToolbox.FindFirst<T>`
- The resolution callback (`Action<Encounter>`, once with a named method, once with a lambda)
- Items: `IItem`, `IUsable`, `IEquippable`, `HealingPotion`, `Sword`
- `IEncounterGenerator` (factory for random encounters on overworld tiles)
- `IBattleResolver` (swappable battle resolution, `AutoBattleResolver` first)
- Wiring `Encounter`/`Party`/battle resolution into the `Dqh.Game` raylib loop
- README with build/run instructions
- Day-2 alternative-track choice (kernekrav's thread race-condition demo vs. one of
  the three alternatives — swap strategy, save/load, or an extra Adventurer tier
  with its own state) — **not decided yet**

## Session log

### 2026-09-09 — Session 1

Done:
- Read the brief PDF, mapped kernekrav a–i onto a Dragon-Quest-style reskin
  (Heltevagten's Hero/Incident/DispatchCenter → Adventurer/Encounter/Party).
- Chose raylib-cs for rendering (confirmed working on this machine: window opens,
  X11/GLFW/OpenGL init succeeds).
- Produced the first-pass `docs/domain-model.drawio`/`.md` UML before coding
  (kernekrav g) — now stale, see above.
- Scaffolded `src/Dqh.Game` (raylib-cs console app) and `tests/Dqh.Domain.Tests`
  (xUnit), wired into `Dqh.slnx`.
- Built a raylib overworld proof-of-concept in `Dqh.Game`: `World/` (`TileMap`,
  `TileType`, `PlayerMarker` — pure data/movement) and `Rendering/`
  (`ITileRenderer`/`CheckerboardTileRenderer`, `IActorRenderer`/`RectangleActorRenderer`
  — interface-first so a bitmap/tileset renderer can replace the flat-color one later),
  `Settings/GridSettings.cs` for grid dimensions. Player moves on a grid with arrow
  keys/WASD. (Committed separately by the user as "project setup and working raylib window".)
- Implemented the full combat core, spell system, and data-driven monster system in
  `Dqh.Domain` — see "What's actually built" above. Design iterated substantially
  through discussion (composition over inheritance for monster/spell variation,
  data-driven monster & spell catalogs instead of one class per kind, DQ-accurate
  vocation names and elemental types, explicit enum values) before landing on what's
  now in the repo.
- Added 13 passing xUnit tests exercising the new domain code.

Next session:
1. Refresh `docs/domain-model.drawio`/`.md` to match the code (see "Design changes"
   above) — do this first, before adding more domain code, per kernekrav g.
2. `Encounter`, `IParty`/`Party` (roster as `List<Adventurer>` aggregation, encounter
   log as `List<Encounter>` composition), `IChampionSelectionStrategy` +
   `FirstAvailableChampionStrategy` injected via `Party`'s constructor (kernekrav h).
3. `DomainToolbox.FindFirst<T>`, the two/three exceptions (including finally wiring
   `InsufficientManaException` into `Adventurer.SpendMana`), the resolution callback.
4. Only after that: items, `IEncounterGenerator`/`IBattleResolver`, wiring encounters
   into the `Dqh.Game` raylib loop, README, and the Day-2 alternative-track choice.
