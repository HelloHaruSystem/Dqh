# Progress log

Running record of work across sessions, kept in the repo so any session can pick up
context without relying on chat history. See [`docs/domain-model.md`](docs/domain-model.md)
for the design itself; this file tracks *build status* against that design.

## Kernekrav checklist (DQH names — see docs/domain-model.md for the mapping)

| Kernekrav | DQH concept | Status |
|---|---|---|
| a) abstract base + polymorphism + encapsulation | `Adventurer` (+ `Monster` mirror) | In progress |
| b) ≥2 self-made interfaces | `ISpellcaster`, `IHealer`, `IPhysicalAttacker`, `IDefender`, `IFleeable` | In progress |
| c) manager class + collections | `Party` / `IParty` | Not started |
| d) generic search method | `DomainToolbox.FindFirst<T>` | Not started |
| e) ≥2 custom exceptions | `AdventurerUnavailableException`, `NoSuitableAdventurerFoundException`, `InsufficientManaException` | Not started |
| f) callback on resolution | `Party.ResolveEncounter(Encounter, Action<Encounter>)` | Not started |
| g) UML before coding | `docs/domain-model.drawio` / `.md` | Done (first pass — will be refined as design evolves) |
| h) dependency inversion via injected strategy | `IChampionSelectionStrategy` → `Party` ctor | Not started |
| i) documentation & git history | README, XML docs, incremental commits | In progress |

## Designed but not yet built (tracked so we don't lose the intent)

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
  X11/GLFW/OpenGL init succeeds, see session notes).
- Designed the fuller domain model (combat core, ability interfaces, encounters/party,
  exceptions, generics, items, encounter-generation/battle-resolution interfaces) —
  richer than the kernekrav floor, per direction to lean heavily on interfaces for
  loose coupling and use design patterns (Strategy, Factory) where they earn their
  place.
- Produced `docs/domain-model.drawio` (authoritative, editable UML) and
  `docs/domain-model.md` (Mermaid preview + design rationale).
- Scaffolded `src/Dqh.Game` (raylib-cs console app) and `tests/Dqh.Domain.Tests`
  (xUnit), wired into `Dqh.slnx`.
- Built a raylib overworld proof-of-concept in `Dqh.Game`: `World/` (`TileMap`,
  `TileType`, `PlayerMarker` — pure data/movement, no rendering) and `Rendering/`
  (`ITileRenderer`/`CheckerboardTileRenderer`, `IActorRenderer`/`RectangleActorRenderer`
  — interface-first so a bitmap/tileset renderer can replace the flat-color one later
  without touching `TileMap`/`PlayerMarker`/`Program.cs`), `Settings/GridSettings.cs`
  for grid dimensions (no magic numbers). Player moves on a grid with arrow keys/WASD.

Next session:
- Implement the combat core in `Dqh.Domain`: `ICombatant`, `Adventurer` (abstract) +
  `Warrior`/`Mage`/`Healer`/`Ranger`, `Monster` (abstract) + `Slime`/`Goblin`/`Dragon`,
  and the ability interfaces (`ISpellcaster`/`IHealer`/`IPhysicalAttacker`/
  `IDefender`/`IFleeable`).
- Then: `Encounter`, `IParty`/`Party`, `IChampionSelectionStrategy`, exceptions,
  `DomainToolbox.FindFirst<T>`, the resolution callback.
