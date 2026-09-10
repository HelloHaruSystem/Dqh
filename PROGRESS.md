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
| h) dependency inversion via injected strategy | `ITargetSelectionStrategy` → `RandomEncounterGenerator` ctor, passed to `Monster` | **Done** |
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
  `ResolveEncounter` (the callback, kernekrav f), `FindAvailableHealer()`/
  `FindFirstUnresolvedEncounter()` (the generic search, kernekrav d, over the
  roster and the encounter log respectively). No longer holds any targeting
  logic — see below.
- **`ITargetSelectionStrategy`/`RandomTargetStrategy`** (kernekrav h): injected
  into `Monster`'s constructor (via `RandomEncounterGenerator`, which is where
  it's actually provided), exposed as `Monster.ChooseTarget(availableTargets)`.
  Targets a uniformly random living party member — matches how DQ's actual
  targeting works (mostly random/formation-weighted, not a computed "best
  target"). Originally lived on `Party` (`Party.ChooseTarget(attacker)`) —
  moved to the monster side after review: "who does a monster attack" is the
  same category of decision as `AttackWeight`/`FleeWeight` (both monster
  behavior), so it belongs with the attacker, not the target. `Party` keeping
  it was a case of parking kernekrav h's required strategy wherever a
  plausible-sounding justification ("party formation") could be written for
  it, not where the decision actually belongs.
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
- **Speed and real Guard, added for the battle system.** `ICombatant.Speed`
  (higher acts first in a round) on every `Adventurer` subclass and every
  `MonsterDefinition` — Ranger/MetalSlime fastest, Priest/Slime slowest,
  matching their DQ archetypes. `Adventurer.IsGuarding`/`ResetGuard()`:
  `Warrior.Guard()` sets it, `Adventurer.TakeDamage` halves the hit while
  it's set — the domain's `IDefender.Guard()` was flavor-text-only before
  this; now it mechanically matters. Both were explicit choices over the
  cheaper alternative (fixed party-then-monster order; flavor-only guard)
  because the point of the exercise was an authentic DQ battle, not the
  least Domain surface area.
- **`Battles/` — the turn-based fight itself.** `BattleCommand` (`Attack`/
  `Cast`/`Heal`/`Guard`/`Flee`, one per living party member per round) →
  `IBattleResolver`/`StandardBattleResolver.ResolveRound(party, monsters,
  commands)`: merges those commands with every living monster into one
  list ordered by `Speed` descending and resolves each in turn, dispatching
  by pattern-matching a command to the ability interface it needs
  (`((ISpellcaster)actor).Cast(...)`, etc.) — every action still returns the
  same flavor-text strings the ability interfaces always did, now collected
  into a `BattleRoundResult.Log`. A monster's own turn rolls
  `AttemptFlee()`/`ChooseTarget()`/`PerformAttack()` exactly as already
  built — the resolver is genuinely just new orchestration over old parts.
  A monster that flees is reported in `FledMonsters`, not removed by the
  resolver itself (kept stateless/reusable — the caller owns the active
  roster). A party member's successful `FleeCommand` ends the round for
  everyone immediately (`PartyFled`), matching classic DQ's "whole party
  runs together," not a per-character escape.
- **`IEncounterGenerator`/`RandomEncounterGenerator`**: picks 1-3 monsters
  uniformly from the whole bestiary for a fresh `Encounter`.
- 37 xUnit tests in `tests/Dqh.Domain.Tests`, all passing (10 new, covering
  Guard's damage halving, Speed ordering, each command type's dispatch, a
  monster flee not being removed by the resolver, and the encounter
  generator's group size).

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
  change mid-loop; `IWorldPresenter`/the renderers just draw whatever
  `GameWorld` currently reports each frame (`IWorldPresenter.Present` was
  later simplified to take the whole `GameWorld` instead of its
  `Map`/`Decorations`/`Npcs` spelled out separately — see the fade-transition
  bullet below, which is what actually forced that). Verified headless:
  walking onto the overworld's door (9,19) lands in the inn at (5,8); walking
  onto the inn's door (5,9) lands back on the overworld at (9,20). Caught and
  fixed a map-data bug along the way: `overworld.json`'s portal targeted the
  inn's own door tile (5,9) instead of its floor tile one
  step off it (5,8) — the inn-to-overworld portal already had this right
  (targets (9,20), not its own door at (9,19)); left uncaught it wouldn't
  have looped, but would've dropped the player exactly on the threshold tile
  instead of just inside.
- **Fade-to-black map transition**: `GameWorld` now owns the transition
  itself, not just the swap — `CheckPortal` starts a fade instead of swapping
  immediately, `GameWorld.Tick(deltaSeconds, player)` ramps `TransitionFade`
  0→1 over `TransitionSettings.FadeSeconds`, performs the actual
  `MapLoader.Load`/`PlayerMarker.WarpTo` at the midpoint (full black), then
  ramps 1→0 on the new map. `GameLoop.Update` calls `world.Tick` every frame
  and skips reading movement input entirely while `world.IsTransitioning` —
  input during the fade is simply not registered (real-time) / stays queued
  untouched (headless), not lost. Forced `IWorldPresenter.Present` to take
  the whole `GameWorld` instead of `Map`/`Decorations`/`Npcs` spelled out
  separately, since a fourth per-frame value (the fade amount) made the
  parameter list the wrong shape to keep extending; `RaylibWorldPresenter`
  draws a black `Raylib.DrawRectangle` over the whole screen at
  `TransitionFade` alpha after everything else. `ConsoleWorldPresenter`
  accepts the same `GameWorld` but doesn't render the fade — headless output
  is otherwise identical, just delayed by the transition's duration (verified
  by counting ticks in the ASCII dump: player position holds at the old tile
  for the fade-out half, jumps to the new tile at the midpoint, holds again
  through fade-in, then resumes moving on the next real input).
- **Face-and-confirm NPC dialogue, via a general `IInteractable`.** `NpcDialogue`
  (static, keyed by NPC id — the behavior `NpcData.Id`'s doc comment always
  said belonged in code, not the map JSON) holds each NPC's lines; `NpcData`
  implements `IInteractable` (`Column`/`Row`/`Interact() -> IReadOnlyList<string>`)
  so the interaction model isn't NPC-specific — a future sign or other prop
  can implement it too, without touching `GameWorld`. `GameWorld` tracks a
  `Queue<string>` of dialogue lines (`ActiveDialogueLine`/`AdvanceDialogue`/
  `IsTalking`) plus `IsFacingInteractable(player)`/`TryInteractWithFaced(player)`,
  which look up whatever tile is immediately in front of the player (from
  `PlayerMarker.Facing`) rather than the tile they're standing on.
  `IInputSource.TryGetConfirm()` (Enter/Space/E in `RaylibInputSource`, an
  `'e'` token in headless scripts via `Input/InputScript.cs`) opens the
  dialogue when facing something interactable, or advances/dismisses it once
  open; movement locks out entirely while talking. `RaylibWorldPresenter`
  draws the active line in a bottom-screen box, and a small "Press Enter to
  chat" pill top-center of the screen whenever the player faces something
  interactable and isn't already talking (`UiSettings`/`Palette` entries);
  `ConsoleWorldPresenter` prints the equivalent as extra lines for headless
  parity.

  This replaced an earlier bump-to-talk cut (walking *into* an NPC
  auto-triggered dialogue) after actually running it surfaced two real bugs:
  (1) the trigger tile was computed as `player.Column + delta` *after*
  `player.Move` already ran, which is only correct when the move was
  blocked — on a successful step toward an adjacent NPC it silently checked
  one tile past where the player actually was, firing dialogue a step early;
  (2) once dialogue locked out movement, a move command already queued ahead
  of the dismiss keystroke could never be drained (`TryGetMove` wasn't being
  called at all while talking), permanently jamming everything behind it —
  a genuine infinite busy-loop, confirmed via traced stderr output (millions
  of identical "peeked a stale queued move" lines in seconds, no crash, no
  further stdin reads). Moving to face-and-confirm sidesteps both: the
  interact check runs continuously off current facing rather than off a
  move's before/after state, and a confirm press is only ever consumed when
  something is actually being interacted with or dismissed.
- **Opening title screen.** `GameWorld.IsShowingWelcome` starts `true`;
  `GameLoop.Update` only polls confirm while it's set, calling
  `DismissWelcome()` on the first press — no movement, no world ticking,
  until then. `RaylibWorldPresenter` draws it as a full-screen opaque
  overlay ("Welcome to DQH" / "Press Enter to begin"); `ConsoleWorldPresenter`
  prints the equivalent and skips the map dump entirely while it's showing.
- **Yes/no dialogue choices; innkeeper stay-the-night flow.** `DialogueStep`
  (`World/DialogueStep.cs`) is either a `DialogueLine` or a `DialogueChoice`
  (prompt + yes/no labels + `Action` callbacks); `GameWorld`'s dialogue queue
  holds a mix of both. `IInteractable.Interact` changed from returning plain
  lines to `void Interact(GameWorld world, PlayerMarker player)` — an
  interactable now drives the world directly (queue lines, queue a choice,
  or both), rather than `GameWorld` just reading static text off it.
  `NpcData.Interact` delegates to a new `NpcBehaviors.Interact(npc, world,
  player)`, which dispatches by id: NPCs with nothing but canned lines still
  go through `NpcDialogue`, but `"innkeeper"` gets real code (`NpcBehaviors`)
  that asks "stay the night?" and, on yes, calls the map-generalized
  `GameWorld.StartRelocation` (below) to move the player beside the inn's
  first bed; on no, queues a decline line instead. `GameWorld.Tick`'s
  transition logic was generalized from "always reloads a target map" to
  an internal `PendingTransition(TargetMap?, TargetColumn, TargetRow)` — a
  portal crossing sets `TargetMap`, a same-map relocation leaves it null, and
  both play the identical fade. Up/down while a choice is shown flips
  `ChoiceYesSelected`; confirm commits it. `RaylibWorldPresenter`/
  `ConsoleWorldPresenter` both got the equivalent choice UI.

  Caught (again) by actually running a headless script, not by reading the
  code: after adding the choice, `GameLoop`'s talking branch called *both*
  `TryGetConfirm()` and `TryGetMove()` every tick unconditionally, in the
  name of never leaving a stray queued move stuck ahead of a confirm
  (session 8's fix). But each headless script line is one tick's worth of
  input — when confirm's call already consumed a tick's real token, the
  second, "just in case" `TryGetMove` call still triggered a fresh blocking
  read of the *next* line, occasionally pulling in a trailing `q` a tick
  early and quitting before an in-flight relocation had finished (so it
  looked like the innkeeper's "yes" silently did nothing, when actually the
  transition just never got the ticks to complete). Fixed by making the two
  calls mutually exclusive per tick — `TryGetMove` is only ever reached when
  `confirmPressed` was false that tick, whether that's to read an up/down
  toggle or, when there's no choice to toggle, to drain a stray move.
- **`GameWorld` split into named collaborators**, addressing the structural
  review from earlier this session (it had accreted transition timing *and*
  dialogue state as raw fields): `World/MapTransition.cs` (fade timing only
  — `Start(targetMap, column, row, onMidpoint)`/`Tick` returns the
  relocation to apply exactly once, at the midpoint; `GameWorld` still owns
  actually applying it, so `MapTransition` doesn't need to know `TileMap`/
  `PlayerMarker` exist) and `World/Conversation.cs` (the dialogue/choice
  queue, lifted out verbatim). `GameWorld` now exposes `Transition`/
  `Conversation` as real properties (`world.Conversation.IsTalking`, not a
  re-wrapped `world.IsTalking`) instead of piling more raw fields onto
  itself for the third mode below.
- **The battle system.** `Program.cs` now actually constructs a `Party`
  (all five classes, named after themselves) — the first point in
  `Dqh.Game` where a `Party` exists at all — plus a
  `RandomEncounterGenerator`/`StandardBattleResolver`, both injected into
  `GameWorld`. `GameWorld.CheckEncounter` (called alongside `CheckPortal`
  after a successful move) rolls `EncounterSettings.TriggerChancePercent`
  (35) on an `EncounterZone` tile; on a hit, it reuses the exact portal
  fade (`MapTransition`, `targetMap: null`, i.e. don't relocate — just cut
  to black) with an `onMidpoint` callback that sets `GameWorld.ActiveBattle`
  instead of moving the player.

  `World/Battle.cs` is the per-fight state machine: for each living party
  member in turn, builds a command menu from which ability interfaces they
  implement (`IAttacker` always → Attack; `+Spell`/`+Heal`/`+Defend`/`+Run`
  for `ISpellcaster`/`IHealer`/`IDefender`/`IFleeable`), any needed
  spell/target submenu via one reusable `World/BattleMenu.cs` cursor, then
  calls `StandardBattleResolver.ResolveRound` once every living member has
  a command. The round's result lines page through their own
  `Conversation` instance (same paging UX as overworld dialogue, reused
  type, separate instance) before the next round starts or the battle ends
  (`Won`/`Lost`/`Fled`). `GameWorld.EndBattle` fades back to the overworld
  the same way it fades in; a `Lost` battle is a forgiving game-over —
  `FullyRestore()` the whole party and warp to `PlayerSpawn` — rather than
  a dead end, since there's no save/load or game-over screen.

  `Rendering/MonsterRenderer.cs` mirrors `NpcRenderer` (`Assets/Monsters/
  {id}.png`, an explicit `MonsterKind → "metal_slime"` map for the one
  non-trivial filename — the same class of bug already hit once with the
  dock worker, avoided this time by writing the map instead of relying on
  `.ToString()`). `RaylibWorldPresenter.Present` swaps the tile/camera view
  for a dedicated battle screen entirely while `ActiveBattle` is set
  (monster row + HP, party HP/MP list, the current menu or log box reusing
  the dialogue box/`DrawChoiceOption` helpers already built);
  `ConsoleWorldPresenter` prints the equivalent for headless parity.

  Caught by actually playing it, not by reading the code: a bug in
  `Battle.ResolveRound` where "no monsters left" was checked as
  `_activeMonsters.All(m => m.IsDefeated)` — `All()` on an empty sequence
  is vacuously true, so a group that entirely *fled* (removed from
  `_activeMonsters`, not defeated) still reported "The monsters have been
  defeated!" Fixed by checking the *original* `_encounter.Monsters` for
  whether anything was actually defeated, to word the outcome accurately
  either way, while still ending the battle as a win either way (nothing
  left to fight is nothing left to fight). Also hit the same
  `TryGetConfirm`/`TryGetMove` double-read ordering bug as sessions 8 and 9
  a third time, this time in the battle branch — fixed the same way
  (`TryGetMove` only reached when confirm wasn't this tick's input), and
  it's now a pattern to check on sight in any new `GameLoop.Update` branch.

  Verified headless end to end, including a real mixed-outcome fight (a
  Slime and two Metal Slimes): per-class menus matched each adventurer's
  actual interfaces, multi-monster target submenu appeared correctly,
  Metal Slime (`Speed` 15, highest) acted and fled before anyone else's
  turn, the Slime was defeated by the party's attacks, the round log paged
  correctly, and the fade back to the overworld landed the player exactly
  where they'd started — with the outcome message correctly reading
  "defeated" rather than "flee" because at least one monster in the group
  actually died.
- **Not yet wired**: the innkeeper actually healing the (now real) `Party`
  — `NpcBehaviors`' innkeeper flow only relocates the player next to the
  bed today; wiring `Party.Members` through to it and calling
  `FullyRestore()` on each is the rest of the work, now that a `Party`
  exists in `Dqh.Game` to heal. Also: items, and the black-void interior
  camera variant.

## Designed but not yet built

- Items: `IItem`, `IUsable`, `IEquippable`, `HealingPotion`, `Sword` — no
  battle-side hook for them yet either (no "Item" option in `Battle`'s menu).
- The innkeeper actually healing the party (`NpcBehaviors` needs to call
  `Adventurer.FullyRestore()` on each `Party.Members` entry — the `Party`
  didn't exist in `Dqh.Game` until the battle system landed, now it does).
- The black-void (unclamped) `Camera` variant for interior scenes.
- Written justification for kernekrav h (why loose coupling makes the targeting
  policy easy to change later) — a few sentences, per kernekrav i.
- Day-2 alternative-track choice (kernekrav's thread race-condition demo vs. one
  of the three alternatives — swap strategy, save/load, or an extra Adventurer
  tier with its own state) — **not decided yet**.

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
1. The innkeeper actually healing the real `Party` that now exists in
   `Dqh.Game` (`Adventurer.FullyRestore()` already exists — the only gap is
   threading `Party.Members` into `NpcBehaviors`' innkeeper flow).
2. Build the black-void camera variant for interior scenes.
3. Items (`IItem`/`IUsable`/`IEquippable`, `HealingPotion`/`Sword`) and an
   "Item" option in `Battle`'s command menu — the battle system itself
   (`Battles/`, `Battle.cs`, encounter triggering) is done.
4. Write the kernekrav h justification paragraph, pick the Day-2 alternative track.
5. Keep `docs/domain-model.md` updated as each of the above lands — it still
   only reflects the pre-battle-system domain shape.

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

### 2026-09-09 — Session 7

Added the fade-to-black transition across a map change (the instant jump-cut
from session 6 "works but we maybe need some animation for when entering and
leaving"). `GameWorld` now drives the fade itself — `CheckPortal` starts it
instead of swapping immediately, `Tick(deltaSeconds, player)` ramps
`TransitionFade` 0→1→0 over two `TransitionSettings.FadeSeconds` halves,
performing the actual map swap/warp at the midpoint (full black), and
`GameLoop.Update` skips movement input entirely while `world.IsTransitioning`
so nothing moves during the fade. This forced `IWorldPresenter.Present` to
change shape — `Map`/`Decorations`/`Npcs` spelled out as three separate
parameters plus a fourth for the fade amount was the wrong direction to keep
extending, so it now just takes the whole `GameWorld`; `RaylibWorldPresenter`
draws a full-screen black rectangle at `TransitionFade` alpha,
`ConsoleWorldPresenter` ignores the fade (headless stays visually
unaffected, same precedent as the camera) but still experiences the same
input lockout, verified by counting ticks in the ASCII dump: player position
holds at the old tile through fade-out, jumps to the new tile at the
midpoint, holds again through fade-in, then moves again on the next real
input.

### 2026-09-09 — Session 8

Added bump-to-talk NPC dialogue. `NpcDialogue` holds each NPC's lines,
in code, keyed by id — matching what `NpcData.Id`'s doc comment already said
("resolves to actual dialogue/behavior in code, not here"). `GameWorld` gained
a dialogue queue (`TrySpeakTo`/`ActiveDialogueLine`/`AdvanceDialogue`/
`IsTalking`); a new `IInputSource.TryGetConfirm()` (Enter/Space/E) advances or
dismisses a line and locks out movement while talking.
`Input/MoveScript.cs` → `Input/InputScript.cs` (adds an `'e'` token) since it
now parses more than movement.

Two real bugs only surfaced by actually running a headless script, not by
reading the code:

- **Wrong trigger tile.** First cut had `GameLoop` call `player.Move(...)`
  then compute the bumped tile as `player.Column + delta` — correct only when
  the move was *blocked* (position unchanged). On a successful step toward an
  adjacent NPC, `player.Column` was already the new position, so adding the
  delta again overshot by one tile and happened to land exactly on the NPC —
  firing dialogue a full step early, mid-approach rather than on the actual
  bump. Fixed by computing the target from the player's position *before*
  calling `Move`.
- **Queue jam → genuine infinite loop.** Once dialogue locked out movement,
  `TryGetMove` was never called at all while talking — so a move command
  already queued ahead of the dismiss keystroke (from the bug above firing
  early, leaving the real bump's move still queued) could never be drained,
  permanently blocking everything behind it. Confirmed via temporary stderr
  tracing in `ConsoleInputSource`: millions of identical "peeked a stale
  queued move, still not a confirm" lines, no crash, no further stdin reads —
  a true busy-loop, not a hang on I/O (`timeout dotnet run` written to a file
  produced ~22 million lines in 20 seconds). Fixed by having `GameLoop` still
  call `TryGetMove` while talking, but discard whatever it returns — draining
  the queue instead of skipping it outright. Both fixes verified together
  with the same headless script that originally reproduced the hang.

### 2026-09-09 — Session 9

Two follow-ups from actually playing session 8's dialogue: swap bump-to-talk
for a facing-and-confirm model, and add an opening title screen.

Introduced `IInteractable` (`Column`/`Row`/`Interact() -> IReadOnlyList<string>`)
per the user's suggestion, specifically so the interaction model isn't
hard-wired to NPCs — a sign or other prop can implement it later without
`GameWorld` caring which concrete type it's looking at. `NpcData` implements
it now (`Interact()` just delegates to the existing `NpcDialogue.LinesFor`).
`GameWorld.TrySpeakTo(column, row)` (bump-triggered) became
`IsFacingInteractable(player)`/`TryInteractWithFaced(player)`, both querying
a new private `FacedTile(player)` (the tile in front of `player.Facing`) and
a `FindInteractableAt(column, row)` that currently only searches NPCs but
doesn't need to know that from the outside. `GameLoop.Update` no longer ties
interaction to a move attempt at all — confirm is checked first each tick;
if pressed, it either opens/advances dialogue or interacts with whatever's
faced, and movement is only attempted when confirm wasn't pressed.

Added the opening title screen the same way as the other blocking overlays
(the fade transition, the dialogue box): a bit of state on `GameWorld`
(`IsShowingWelcome`, default `true`) that `GameLoop.Update` checks first and
returns early on until dismissed. `Settings/DialogueSettings.cs` renamed to
`UiSettings.cs` and gained the interact-prompt and title font/layout
constants, since by now it covered more than just the dialogue box.

Verified headless end to end: `e` dismisses the welcome screen, walking to
and bumping the innkeeper shows the "facing something" prompt (not
dialogue), a second `e` opens the dialogue, a third dismisses it back to the
prompt (still facing them) — and `q` during the welcome screen quits cleanly
instead of hanging, since the outer loop's quit check doesn't depend on
`GameWorld` state at all.

### 2026-09-09 — Session 10

Added yes/no dialogue choices and the innkeeper's stay-the-night flow — see
"What's actually built" above for the final shape (`DialogueStep`,
`IInteractable.Interact` now driving `GameWorld` directly, `NpcBehaviors`,
`GameWorld.StartRelocation` generalizing the portal transition to same-map
moves). Caught the same class of bug as session 8 one layer up: calling both
`TryGetConfirm`/`TryGetMove` unconditionally per tick could over-read a
headless script by one line, quitting before an in-flight relocation
finished — fixed by making the two mutually exclusive per tick. Verified
both the "yes" (relocates beside the bed) and "no" (decline line, no
relocation) paths headless, each quitting cleanly.

Flagged for a later session, not done now: the user asked for a look at
whether the accumulated `Dqh.Game` changes (animation, NPCs, portals,
transitions, dialogue, choices, welcome screen — sessions 4 through 10) are
still well-structured, given how much has landed without a dedicated pass to
step back and check. That review is still pending.

### 2026-09-10 — Session 11

Two fixes from a user code-review pass, one at a time:

- **Battle screen now states whose turn it is.** `Battle.CurrentActor` (set
  whenever a party member's main/spell/target menu is up, cleared once the
  round resolves) is drawn as "{name}'s turn" above the command menu — both
  `RaylibWorldPresenter` and `ConsoleWorldPresenter`. Verified headless with a
  scripted fight: the label stepped Hero → Warrior → Mage → Priest → Ranger
  correctly each round.
- **Moved monster targeting off `Party`, onto `Monster`.** Flagged by the user
  as a code smell: `Party.ChooseTarget(attacker)` held the injected
  `ITargetSelectionStrategy` and decided targets on the *monster's* behalf,
  which is backwards (the monster is the one attacking) and inconsistent with
  `AttackWeight`/`FleeWeight` already modeling monster behavior on the monster
  side. `ITargetSelectionStrategy` now injects into `Monster` (constructor,
  optional param defaulting to `RandomTargetStrategy` so the ~27 other
  `Monster.Create` call sites that don't care about targeting don't need to
  supply one); `Monster.ChooseTarget(availableTargets)` mirrors the existing
  `AttemptFlee()` self-contained-decision shape. The real injection point is
  `RandomEncounterGenerator`'s constructor, which passes its strategy to every
  `Monster.Create` it calls. `Party`/`IParty` dropped `ChooseTarget` and the
  strategy entirely — back to just roster/encounter-log bookkeeping.
  `StandardBattleResolver` now computes the living-members list itself and
  calls `monster.ChooseTarget(standing)`. Test fallout: `PartyTests`' two
  `ChooseTarget` tests moved to a new `Combatants/MonsterTargetSelectionTests.cs`,
  rewritten against `Monster` instead of `Party`; everything else was a
  mechanical constructor-signature update. 37 tests still pass (2 moved, not
  lost). `docs/domain-model.md`'s diagram and kernekrav-h rationale updated to
  match — the old "party-side, belongs to formation" justification is called
  out as having been a rationalization for parking the required strategy
  somewhere, not a reflection of an actual formation mechanic (none exists).
