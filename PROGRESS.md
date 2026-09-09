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
- **`Adventurer.FullyRestore()`** (heals HP and mana to max, safely — via the
  existing `Heal`/`RestoreMana`, not `int.MaxValue`) and **`IParty`/`Party.Members`**
  (read-only roster view) — added for the Inn's innkeeper NPC in `Dqh.Game`.
- 27 xUnit tests in `tests/Dqh.Domain.Tests`, all passing.

## What's actually built (`src/Dqh.Game`)

- **Headless vs. windowed, one composition root.** `Program.cs` picks
  `ConsoleInputSource`/`ConsoleWorldPresenter`/`HeadlessClock` or
  `RaylibInputSource`/`RaylibWorldPresenter`/`RaylibClock` based on a
  `--headless` flag; `GameLoop` (shared, no Raylib reference) only knows the
  `IInputSource`/`IWorldPresenter`/`IClock` interfaces. Headless mode takes
  moves as stdin lines (`w`/`a`/`s`/`d`, `q` to quit) and dumps the whole map as
  ASCII every tick — deliberately unaffected by the camera, so it stays fully
  scriptable/testable.
- **`IClock`/delta time**: `RaylibClock.DeltaSeconds => Raylib.GetFrameTime()`,
  `HeadlessClock.DeltaSeconds` is a nominal 1/60s tick — introduced specifically
  so `GameLoop`/input timing logic never needs a Raylib reference.
- **Key-repeat input**: tap = instant move (`IsKeyPressed`), hold = timed repeat
  every `MoveRepeatIntervalSeconds` (0.15f) using accumulated delta time — plain
  `IsKeyDown` fired every frame (far too fast); a single `IsKeyPressed` check
  only fires once per press (can't hold a direction). Feels "a little fast" per
  playtesting — flagged to retune once more of the game exists to judge pacing
  against, not yet done.
- **Real texture rendering**: `TexturedTileRenderer`/`SpriteActorRenderer` load
  PNGs from `Assets/{Tiles,Characters}` (path resolved via
  `AppContext.BaseDirectory`, robust regardless of working directory) — replaced
  the earlier flat-color placeholder renderers entirely.
- **Maps as data, Tiled-editor-style**: `MapLoader` reads a `{map}.csv` (tile
  index grid) + `{map}.json` (`MapEntities`: player spawn, NPCs, portals,
  decorations — position/id only, behavior resolved in code from the id) per
  map name from `Assets/Maps`. `overworld` (40x28) and `inn` (11x10) both exist.
- **Camera**: `Rendering/Camera.cs` — a `GridSettings.CameraColumns`x`CameraRows`
  (13x9) window centered on the player, clamped to the map's bounds
  (`Camera.Follow`). `ITileRenderer`/`IActorRenderer` both take the `Camera` and
  draw only its visible slice, converting map-absolute to camera-relative
  coordinates before pixel mapping. The overworld's mountain/river borders were
  deliberately made *thick* (6 tiles) specifically so a full camera window's
  depth never sees open terrain through to the other side — validated via
  rendered preview crops before any code was written. Console/headless mode is
  unaffected by the camera (always dumps the whole map). An unclamped
  "black-void" variant for interior scenes (so the Inn's walls don't reveal a
  clamped edge) is designed but **not yet built**.
- **`TileType`/`TileTraits`**: 14 explicit tile values (terrain + interior
  furniture — `Door`/`Floor`/`Wall`/`BarCounter`/`BedHead`/`BedFoot`/`Table`/etc.
  are baked into the CSV like terrain, since they're solid tile-grid objects).
  `TileTraits.IsWalkable(this TileType)` (extension-method syntax, by explicit
  preference) is the single source of truth for which tiles block movement.
- **Decorations block movement via `TileMap`, not a parallel check.**
  `TileMap` now owns a small `_blockedPositions` set alongside its terrain grid:
  `IsWalkable(column, row)` combines `GetTile(...).IsWalkable()` with "not
  occupied", and `Block(column, row)` marks a cell solid regardless of terrain.
  `MapLoader` calls `Block` for every loaded decoration. `PlayerMarker.Move`
  changed from `_map.GetTile(...).IsWalkable()` to `_map.IsWalkable(...)` — one
  line, no new logic in `PlayerMarker` itself. This was a deliberate correction:
  the first attempt hand-rolled a decoration-scanning loop directly inside
  `PlayerMarker.Move`, which duplicated the walkability concept that
  `TileTraits` already owns instead of extending it.
- **Decoration rendering**: `PropRenderer` loads `Assets/Props/{id}.png` per
  decoration id (lazily, cached), draws each one culled to the camera window.
  Kept as a JSON entity layer (not baked into the tile CSV like furniture)
  because it's positioned/rendered independently of terrain — the deliberate
  choice, when asked, was to extend `TileMap` to also block these positions
  rather than convert decorations into another `TileType`.
- Original pixel art (Python + Pillow, 16x16 logical canvas, 8x nearest-neighbor
  upscale, no anti-aliasing) for all tiles, the hero (3-direction/2-frame sheet,
  "Right" = "Left" flipped at draw time, not a baked frame), NPCs, monsters, and
  the sign prop — reviewed via `Assets/temp/` renders before finalizing.
- **Not yet wired**: `Scene`/`Portal`/`Npc`/`GameWorld` (map-to-map transitions,
  bump-to-talk NPC interaction, the innkeeper actually healing a `Party`) —
  designed in an earlier plan-mode session, predates the asset-first pivot, and
  still needs to be built against what actually exists now.

## Designed but not yet built

- Items: `IItem`, `IUsable`, `IEquippable`, `HealingPotion`, `Sword`
- `IEncounterGenerator` (factory for random encounters on overworld tiles)
- `IBattleResolver` (swappable battle resolution, using `Party.ChooseTarget` and
  `Monster.AttemptFlee`/`PerformAttack` to actually run a fight turn by turn)
- `Scene`/`Portal`/`Npc`/`GameWorld` in `Dqh.Game` — map-to-map transitions
  (walking through the Inn's door and back), bump-to-talk NPC interaction, the
  innkeeper healing a constructed `Party` via `Adventurer.FullyRestore()`, the
  dock worker's "no boats today" line. Assets/maps/camera/movement are all in
  place for this now; it's purely the wiring left.
- The black-void (unclamped) `Camera` variant for interior scenes
- Retuning `MoveRepeatIntervalSeconds` (currently 0.15f, feels "a little fast")
  now that the camera gives a real sense of on-screen scale
- Wiring `Encounter`/`Party`/battle resolution into the `Dqh.Game` raylib loop
  (stepping onto an `EncounterZone` tile should trigger a fight)
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

### 2026-09-09 — Session 3 (`real_textures` → `camera` branches)

Built the actual playable slice's foundations in `Dqh.Game`, working slowly in
small independently-verified chunks (build + domain tests + headless smoke test
after each). Also reworded several early commit messages via
`git rebase -i --rebase-merges` (file-based `GIT_SEQUENCE_EDITOR`/`GIT_EDITOR`
scripts — inline shell strings silently flattened the merge topology; writing
them as real script files fixed it, verified via the previewed todo list and
`git log --min-parents=2` still showing all merges).

Sequence: generated all original pixel art (Pillow, reviewed via `Assets/temp/`
before finalizing) → gave assets meaningful directory/file names → `MapLoader`
(Tiled-style CSV+JSON) → `IClock`/delta-time plumbing → real texture rendering
(`TexturedTileRenderer`/`SpriteActorRenderer`) → `Program.cs`/`GameLoop` cleanup
(was bloated, now a clean composition root + Update/Draw loop) → fixed
janky/tap-only movement (key-repeat via delta time) → camera (`Camera.Follow`,
clamped 13x9 window) → decoration rendering (`PropRenderer`) → decorations
blocking movement.

Two corrections worth remembering:
- The overworld's impassable border was first widened while keeping grass
  right up to the mountain/river edge — didn't fix anything, since the player
  can walk on grass and would still see past a thin border. The actual fix was
  making the *impassable* mountain/river bands themselves 6 tiles thick, so a
  full camera window's depth never sees through to open terrain.
- When the inn sign (a decoration) turned out to be walkable, the first fix
  hand-rolled a decoration-position scan inside `PlayerMarker.Move` — a second,
  parallel walkability check next to the one `TileTraits.IsWalkable` already
  owns. Corrected to extend `TileMap` itself (`IsWalkable`/`Block`) so
  `PlayerMarker` keeps its one existing check, now covering both terrain and
  occupancy. Considered (and rejected, by choice) converting the sign into a
  `TileType` like `Table`/`Door` — decorations stay a separate JSON entity
  layer since they're rendered independently of terrain.

Next session:
1. Build `Scene`/`Portal`/`Npc`/`GameWorld` — the Inn door transition,
   bump-to-talk NPCs, innkeeper healing a real `Party`, dock worker's line.
2. Build the black-void camera variant for interior scenes.
3. Retune `MoveRepeatIntervalSeconds` once the camera's scale makes pacing
   judgable.
4. Items (`IItem`/`IUsable`/`IEquippable`), `IEncounterGenerator`, `IBattleResolver`
   (using the pieces already built: `Party.ChooseTarget`, `Monster.AttemptFlee`),
   then wire encounter/battle resolution into `Dqh.Game` so stepping onto an
   `EncounterZone` tile triggers a fight.
5. Write the kernekrav h justification paragraph, pick the Day-2 alternative track.
6. Keep `docs/domain-model.md` updated as each of the above lands.
