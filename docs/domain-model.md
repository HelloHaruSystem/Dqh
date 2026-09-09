# Domain model

The combat core as built in `src/Dqh.Domain` (kernekrav a/b). `Encounter`/`Party`/
strategy/exceptions/etc. (kernekrav c–h) aren't built yet — see
[`../PROGRESS.md`](../PROGRESS.md) for what's next.

```mermaid
classDiagram
    class ICombatant {
        <<interface>>
        +string Name
        +int MaxHitPoints
        +int CurrentHitPoints
        +bool IsDefeated
        +TakeDamage(amount) void
        +Heal(amount) void
    }
    class HitPointTrack {
        <<internal>>
        +int MaxHitPoints
        +int Current
        +bool IsDefeated
        +TakeDamage(amount) void
        +Heal(amount) void
    }

    class Adventurer {
        <<abstract>>
        -int _mana
        +int Mana
        +SpendMana(amount) void
        +RestoreMana(amount) void
        +UseSignatureMove(target)* string
    }
    Adventurer ..|> ICombatant
    Adventurer *-- HitPointTrack : hit points
    Adventurer <|-- Hero
    Adventurer <|-- Warrior
    Adventurer <|-- Mage
    Adventurer <|-- Priest
    Adventurer <|-- Ranger

    class IAttacker { <<interface>> +PerformAttack(target) string }
    class ISpellcaster { <<interface>> +KnownSpells +Cast(spell, target) string }
    class IHealer { <<interface>> +KnownHealingSpells +Heal(spell, target) string }
    class IDefender { <<interface>> +Guard() string }
    class IFleeable { <<interface>> +AttemptFlee() bool }

    Hero ..|> IAttacker
    Warrior ..|> IAttacker
    Warrior ..|> IDefender
    Mage ..|> ISpellcaster
    Priest ..|> IHealer
    Ranger ..|> IAttacker
    Ranger ..|> IFleeable

    class IMonster {
        <<interface>>
        +MonsterKind Kind
        +int AttackPower
        +int DefensePower
        +IsWeakTo(element) bool
    }
    IMonster --|> ICombatant

    class MonsterKind { <<enumeration>> Slime=1 Dracky=2 Ghost=3 }
    class MonsterDefinition {
        +MonsterKind Kind
        +string Name
        +int MaxHitPoints
        +int AttackPower
        +int DefensePower
        +ElementType AttackElement
        +Weaknesses
        +string AttackDescriptionTemplate
    }
    class MonsterBestiary { <<internal, static>> +Get(kind) MonsterDefinition }
    class Monster { +Create(kind)$ Monster +PerformAttack(target) string +IsWeakTo(element) bool }

    Monster ..|> IMonster
    Monster ..|> IAttacker
    Monster *-- HitPointTrack : hit points
    Monster o-- MonsterDefinition : stats
    Monster ..> MonsterBestiary : Create() looks up
    MonsterBestiary *-- MonsterDefinition : catalog
    MonsterBestiary ..> MonsterKind : keyed by

    class ElementType { <<enumeration>> Physical=1 Fire=2 Ice=3 Wind=4 Explosion=5 }
    class ISpell { <<interface>> +string Name +int ManaCost +Apply(caster, target) string }
    class DamageSpell { +ElementType Element +Apply(caster, target) string }
    class HealingSpell { +Apply(caster, target) string }
    class SpellId { <<enumeration>> Ember=1 Inferno=2 Mend=3 GreaterMend=4 }
    class SpellBook { <<internal, static>> +GetDamageSpell(id) DamageSpell +GetHealingSpell(id) HealingSpell }

    DamageSpell ..|> ISpell
    HealingSpell ..|> ISpell
    DamageSpell ..> IMonster : checks weakness
    SpellBook *-- DamageSpell : catalog
    SpellBook *-- HealingSpell : catalog
    SpellBook ..> SpellId : keyed by
    Mage o-- DamageSpell : known spells
    Mage ..> SpellBook : looks up
    Priest o-- HealingSpell : known spells
    Priest ..> SpellBook : looks up
```

**Reskin:** the brief's superhero dispatch case, renamed — `Hero`→`Adventurer`,
`Incident`→`Encounter` (not built yet), `DispatchCenter`→`Party` (not built yet).

## Why it's shaped this way

- **`ICombatant` interface, no shared base class.** `Adventurer` and `Monster` only
  share HP bookkeeping (composed via `HitPointTrack`, not inherited) — everything
  else diverges. Shared *capability*, not shared *taxonomy*: a Duck and an Airplane
  both `CanFly` without a common `Flyer` base.
- **Monsters are one data-driven class, not a subclass per monster.**
  `MonsterKind` → `MonsterDefinition` → `MonsterBestiary` → single `Monster` class.
  Same pattern for spells (`ISpell` + `DamageSpell`/`HealingSpell`, no abstract
  `Spell` base) via `SpellId` → `SpellBook`.
- **Aggregation vs. composition, applied literally.** Catalogs (`MonsterBestiary`,
  `SpellBook`) *compose* their entries — they own them. `Monster`/`Mage`/`Priest`
  only *aggregate* a shared entry looked up from a catalog — they don't own it
  exclusively. `HitPointTrack` is genuinely composed (private, dies with its owner).
- **Ability interfaces cut across the hierarchy.** `IAttacker` is implemented by
  three `Adventurer` subclasses *and* the unrelated `Monster` — capability, not
  position in a type tree. `ISpellcaster`/`IHealer` are typed to their specific
  spell class, so the compiler blocks casting a heal spell through the attack path.
- **Elemental weaknesses are mechanical, not flavor.** `ElementType` mirrors DQ's
  spell families (Fire/Ice/Wind/Explosion + Physical). `DamageSpell.Apply` doubles
  damage when the target `IMonster` is weak to its element.
- **Access modifiers, restrictive by default.** Leaf classes `sealed`; internal
  helpers (`HitPointTrack`, `MonsterBestiary`, `SpellBook`) `internal`; mutable
  state is a public getter behind a `private` setter, changed only via validated
  methods. Enums use explicit 1-based values so reordering can't renumber them.

See [`../PROGRESS.md`](../PROGRESS.md) for build status.
