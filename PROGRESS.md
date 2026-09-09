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
  every `MovementSettings.StepIntervalSeconds` using accumulated delta time —
  plain `IsKeyDown` fired every frame (far too fast); a single `IsKeyPressed`
  check only fires once per press (can't hold a direction). Originally 0.15f
  and flagged as "feels a little fast" — retuned to 0.25f once the walk
  animation (below) made the old pace read as an outright flicker.
- **Hero walk-cycle animation**: `PlayerMarker` now tracks `Facing` (a
  `Direction`: Down/Up/Left/Right) and `WalkFrame` (0/1), updated by `Move` —
  facing changes even on a blocked move (bumping a wall turns you to face it
  without animating a step), `WalkFrame` flips only on an actual step and
  resets to standing after `MovementSettings.WalkAnimationIdleResetSeconds` of
  no stepping (`PlayerMarker.Tick`, called every `GameLoop.Update`).
  `SpriteActorRenderer` picks the sprite row from `Facing` (Down/Up/Left map to
  rows 0/1/2 of `hero.png`; Right reuses row 2 mirrored via a negative-width
  source `Rectangle` in `DrawTexturePro` — there's no separate Right row in the
  sheet) and the column from `WalkFrame`. `MovementSettings` is the single
  source for both the input-repeat interval and the animation's idle-reset
  threshold (derived from it, `* 1.5`) specifically so they can't drift apart
  and reintroduce the jitter/flicker problem above.
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
  "Right" = "Left" flipped at draw time, not a baked frame — now actually wired
  up, see the walk-cycle animation bullet above), NPCs, monsters, and the sign
  prop — reviewed via `Assets/temp/` renders before finalizing.
- **NPC rendering**: `NpcRenderer` mirrors `PropRenderer` exactly (per-id
  texture cached from `Assets/Characters`, culled to camera) — same shape of
  problem, kept as its own class rather than generalized together, matching
  the existing precedent of separate single-purpose renderers
  (`TexturedTileRenderer`/`PropRenderer`/`SpriteActorRenderer`) even though the
  draw loop is textually similar; NPCs are static single-frame portraits (no
  direction/walk sheet like the hero's), and are expected to diverge from
  decorations once bump-to-talk/dialogue lands. `MapLoader.Load` now also
  calls `TileMap.Block` for each NPC position (same one-liner already used for
  decorations), so a rendered NPC is solid — the player can no longer walk
  through it invisibly. `ConsoleWorldPresenter`'s ASCII dump got matching
  symbols (`I` innkeeper, `W` dock worker) for headless parity.
- **Map-to-map portals**: `World/GameWorld.cs` owns the currently-loaded map —
  `Map`/`Decorations`/`Npcs`/`PlayerSpawn`, all delegating to whatever
  `MapLoader.Load` last returned. `GameWorld.CheckPortal(player)` (called from
  `GameLoop.Update` right after a successful `player.Move`) checks the
  player's new tile against the current map's `Portals`; on a match it
  reloads the target map (re-running `MapLoader.Load`, which re-blocks that
  map's own decorations/NPCs for free) and calls the pre-existing
  `PlayerMarker.WarpTo`. `Program.cs`/`GameLoop.Run` now pass a `GameWorld`
  instead of a fixed `TileMap`/decorations/NPCs triplet, so the active map can
  change mid-loop; `IWorldPresenter`/the renderers are untouched; they still
  just draw whatever `GameWorld` currently reports each frame. Verified
  headless: walking onto the overworld's door (9,19) lands in the inn at
  (5,8); walking onto the inn's door (5,9) lands back on the overworld at
  (9,20). Caught and fixed a map-data bug along the way: `overworld.json`'s
  portal targeted the inn's own door tile (5,9) instead of its floor tile one
  step off it (5,8) — the inn-to-overworld portal already had this right
  (targets (9,20), not its own door at (9,19)); left uncaught it wouldn't
  have looped, but would've dropped the player exactly on the threshold tile
  instead of just inside.
- **Not yet wired**: bump-to-talk NPC interaction (dialogue, the innkeeper
  actually healing a `Party`, the dock worker's line) — designed in an
  earlier plan-mode session, predates the asset-first pivot. NPCs render,
  block movement, and portals move the player between maps; nothing happens
  yet when the player bumps into an NPC specifically.

## Designed but not yet built

- Items: `IItem`, `IUsable`, `IEquippable`, `HealingPotion`, `Sword`
- `IEncounterGenerator` (factory for random encounters on overworld tiles)
- `IBattleResolver` (swappable battle resolution, using `Party.ChooseTarget` and
  `Monster.AttemptFlee`/`PerformAttack` to actually run a fight turn by turn)
- Bump-to-talk NPC interaction, the innkeeper healing a constructed `Party`
  via `Adventurer.FullyRestore()`, the dock worker's "no boats today" line —
  map-to-map transitions themselves (`GameWorld`/portals) are now built; this
  is specifically about the player bumping into an NPC's tile.
- The black-void (unclamped) `Camera` variant for interior scenes
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
1. Bump-to-talk NPC interaction — dialogue, innkeeper healing a real `Party`,
   dock worker's line. Map-to-map transitions (`GameWorld`/portals) are done.
2. Build the black-void camera variant for interior scenes.
3. Items (`IItem`/`IUsable`/`IEquippable`), `IEncounterGenerator`, `IBattleResolver`
   (using the pieces already built: `Party.ChooseTarget`, `Monster.AttemptFlee`),
   then wire encounter/battle resolution into `Dqh.Game` so stepping onto an
   `EncounterZone` tile triggers a fight.
4. Write the kernekrav h justification paragraph, pick the Day-2 alternative track.
5. Keep `docs/domain-model.md` updated as each of the above lands.

### 2026-09-09 — Session 4

Built the hero's walk-cycle animation: `PlayerMarker` gained `Facing`
(`Direction`: Down/Up/Left/Right) and `WalkFrame` (0/1), `SpriteActorRenderer`
picks the sprite row/column from those and mirrors the Left row for Right (no
separate Right row in `hero.png`). First pass tied the walk-frame reset
straight to the pre-existing `MoveRepeatIntervalSeconds` (0.15f) in
`RaylibInputSource` via a second, independent constant in `PlayerMarker` —
playtesting immediately called it "hurts to look at, doesn't seem smooth".
Root cause: two uncoordinated magic numbers pacing the same underlying step
cadence with barely any margin between them (0.15f vs. 0.2f), at a pace fast
enough (~6.7 steps/sec) that position-pop + leg-flip together read as a
strobe. Fixed by consolidating both into one `Settings/MovementSettings.cs`
(`StepIntervalSeconds`, with the idle-reset threshold derived from it at
`* 1.5` so they can't drift apart again) and slowing the shared interval to
0.25f — closing the "retune `MoveRepeatIntervalSeconds`" item that had been on
the backlog since session 3. Deliberately did *not* add tile-to-tile sliding
or smooth camera scrolling to chase extra smoothness: original Dragon Quest
movement is instant tile-snap too, so that would've been a much bigger change
(`Camera`/`Viewport` would need fractional positions) in service of an
un-DQ-authentic feel, not the actual bug.

### 2026-09-09 — Session 5

Rendered the NPCs (`innkeeper`, `dock_worker`) that were already sitting in the
map JSON with no visual. `NpcRenderer` mirrors `PropRenderer`'s shape (per-id
cached texture from `Assets/Characters`, culled to camera) — kept as its own
class rather than unifying with `PropRenderer` despite the near-identical draw
loop, since NPCs are expected to diverge once bump-to-talk/dialogue lands and
the codebase already prefers several small single-purpose renderers over one
generalized one. Plumbed `NpcData` through `GameLoop`/`IWorldPresenter`/both
presenters/`Program.cs` the same way `DecorationData` already flows.
`MapLoader.Load` now also calls the existing `TileMap.Block` for each NPC
position (the same one-liner already used for decorations) — without it, a
now-visible NPC would let the player walk straight through it. Bump-to-talk
interaction itself stays out of scope, deferred to a later session.

Shipped with a bug: `overworld.json`'s dock worker was `"id": "dockWorker"`,
but the actual asset (like every other multi-word asset in the project —
`bar_counter.png`, `bed_head.png`, `encounter_zone.png`) is snake_case,
`dock_worker.png`. `Raylib.LoadTexture` on a missing path fails silently
(a 0x0 texture that draws nothing) rather than throwing, so the NPC blocked
movement correctly but rendered as nothing — caught via playtesting
("it doesn't render but it blocks the worker"), fixed by renaming the id to
`dock_worker` to match convention rather than renaming the asset.

### 2026-09-09 — Session 6

Wired map-to-map portals: `World/GameWorld.cs` owns whichever map is
currently loaded (`Map`/`Decorations`/`Npcs`/`PlayerSpawn`, delegating to
`MapLoader.Load`'s result) and `CheckPortal(player)` swaps it — reloading the
target map (which re-blocks its own decorations/NPCs for free, reusing
`MapLoader.Load` as-is) and calling the already-existing
`PlayerMarker.WarpTo`. `GameLoop.Update` calls it right after a successful
`player.Move`. `Program.cs`/`GameLoop.Run` now thread a `GameWorld` through
instead of a fixed `TileMap`/decorations/NPCs triplet — `IWorldPresenter` and
every renderer are untouched, since they only ever drew whatever they were
handed per frame, not the map itself. Caught a map-data bug while verifying:
`overworld.json`'s portal targeted the inn's own door tile (5,9) instead of
its floor tile one step off it (5,8) — asymmetric with the inn's own portal,
which already correctly targets the overworld's floor at (9,20) rather than
its own door at (9,19). Verified both directions headless: walking onto
(9,19) lands in the inn at (5,8); walking onto the inn's (5,9) lands back at
(9,20).
