# Progress log

Running record of work across sessions, kept in the repo so any session can pick up
context without relying on chat history. See [`docs/domain-model.md`](docs/domain-model.md)
for the design; this file tracks *build status* against that design.

The UML lives as a Mermaid diagram embedded in `docs/domain-model.md` (switched from
an earlier hand-placed draw.io XML file, since Mermaid's automatic layout produced a
much cleaner result — see the session log). It was refreshed at the end of session 1
to match the combat core as actually built. Keep this discipline: refresh the
diagram whenever the domain design changes, not just once at the end.

## Kernekrav checklist (DQH names)

| Kernekrav | DQH concept | Status |
|---|---|---|
| a) abstract base + polymorphism + encapsulation | `Adventurer` (abstract) + `Hero`/`Warrior`/`Mage`/`Priest`/`Ranger` | **Done** |
| b) ≥2 self-made interfaces | `IAttacker`, `ISpellcaster`, `IHealer`, `IDefender`, `IFleeable` | **Done** |
| c) manager class + collections | `Party` / `IParty` | Not started |
| d) generic search method | `DomainToolbox.FindFirst<T>` | Not started |
| e) ≥2 custom exceptions | `AdventurerUnavailableException`, `NoSuitableAdventurerFoundException`, `InsufficientManaException` | Not started (see note below) |
| f) callback on resolution | `Party.ResolveEncounter(Encounter, Action<Encounter>)` | Not started |
| g) UML before coding | `docs/domain-model.md` (Mermaid) | **Done** — matches current code |
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
- `Combatants/` reorganized into subfolders once it grew past ~10 files: shared
  contract (`ICombatant`, `HitPointTrack`) stays at `Combatants/` root,
  `Combatants/Adventurers/` holds the hero hierarchy, `Combatants/Monsters/` holds
  the monster system — namespaces match folders (`Dqh.Domain.Combatants.Adventurers`
  / `.Monsters`).
- `docs/domain-model.md` refreshed to match all of the above.

## Design changes since the first UML pass (now reflected in the diagram)

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
- Produced the first-pass UML before coding (kernekrav g), initially as a
  hand-placed draw.io XML file.
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

- Reorganized `Combatants/` into `Adventurers/`/`Monsters/` subfolders (namespaces
  matching folders) once the flat folder grew past ~10 files mixing three
  sub-concerns.
- Refreshed the UML to match the fully implemented combat core (still the
  speculative first pass before this) — as a draw.io XML file first, but its
  hand-placed coordinates produced edges cutting through boxes with no way to
  preview the result. Switched to Mermaid instead: real automatic layout (dagre),
  renders natively in the diagramming Artifact, on GitHub, and in most editors.
  Deleted `docs/domain-model.drawio`; `docs/domain-model.md`'s Mermaid diagram is
  now the sole UML deliverable. Also trimmed the surrounding prose in that file
  down to short bullets per request.
- Decoupled `Dqh.Game` from Raylib so a headless mode is possible (also makes the
  game loop scriptable/testable without a window): `Input/IInputSource` →
  `RaylibInputSource` (keyboard) / `ConsoleInputSource` (reads stdin one line at a
  time, each line parsed via `MoveScript` into a move queue; blank line/EOF/"q"/
  "quit" requests quit) + `MoveScript` (WASD-string parser); `Presentation/
  IWorldPresenter` → `RaylibWorldPresenter` / `ConsoleWorldPresenter` (ASCII grid
  dump); `GameLoop.Run` is the shared, Raylib-agnostic tick loop; `Program.cs` is
  now just the composition root picking `--headless` vs. windowed mode. First cut
  of headless mode took its whole script as one CLI argument (`ScriptedInputSource`)
  — not actually a loop, just a one-shot batch. Replaced with the real stdin loop
  above per feedback. Verified: piping moves line-by-line traces correctly and
  quits on `q`; EOF alone (no explicit quit) also terminates cleanly; windowed mode
  still opens/inits fine.
  Deliberately did **not** put `TileMap`/`PlayerMarker` behind interfaces — they're
  game state, not swappable policy, so an interface there would've been
  abstraction with no second implementation in sight.
- Made the Raylib window resizable (`dynamic_window_sizing` branch):
  `Rendering/Viewport` (a `readonly record struct`) computes tile size + centering
  offset each frame by fitting `GridSettings.Columns × Rows` into the *current*
  `Raylib.GetScreenWidth()/GetScreenHeight()` — square tiles, letterboxed, not
  stretched. `ITileRenderer`/`IActorRenderer.Draw` now take a `Viewport` instead of
  reading the fixed `GridSettings.TileSizePixels` constant directly;
  `RaylibWorldPresenter` recomputes it every `Present()` call.
  `Raylib.SetConfigFlags(ConfigFlags.ResizableWindow)` + `SetWindowMinSize` (new
  `GridSettings.MinTileSizePixels`/`MinWindow*` constants) enable and floor the
  resize. `ConsoleWorldPresenter` (headless) is untouched — it has no window to
  resize. Verified: build clean, windowed mode still inits, headless trace
  unaffected, all 13 domain tests still pass.

Next session:
1. `Encounter`, `IParty`/`Party` (roster as `List<Adventurer>` aggregation, encounter
   log as `List<Encounter>` composition), `IChampionSelectionStrategy` +
   `FirstAvailableChampionStrategy` injected via `Party`'s constructor (kernekrav h).
2. `DomainToolbox.FindFirst<T>`, the two/three exceptions (including finally wiring
   `InsufficientManaException` into `Adventurer.SpendMana`), the resolution callback.
3. Only after that: items, `IEncounterGenerator`/`IBattleResolver`, wiring encounters
   into the `Dqh.Game` raylib loop, README, and the Day-2 alternative-track choice.
4. Keep `docs/domain-model.md` updated as each of the above lands, rather than
   batching the refresh at the end again.
