# CLAUDE.md

## Git

The user handles all non-read-only git operations themselves (staging, committing,
branching, pushing, etc.). Only use read-only git commands (`status`, `diff`, `log`,
`branch` without creating/deleting, etc.) — never `add`, `commit`, `push`,
`checkout`/`switch`, `branch -d`, `merge`, `rebase`, or similar, even if asked to
"save" or "finish up" work. Leave the working tree as-is and let the user decide
when/how to commit.

## Design domain concepts from real Dragon Quest mechanics, not the original brief's shape

This project reskins a "superhero dispatch center" assignment brief (Heltevagten)
as a Dragon-Quest-style game. The brief only fixes which **C# techniques** must be
demonstrated (abstract base + polymorphism, ≥2 self-made interfaces, a
collections-holding manager class, a generic search method, ≥2 custom exceptions,
a resolution callback, an injected strategy for dependency inversion). It does
**not** fix what those techniques attach to — don't translate the brief's
incident-dispatch concepts (an incident has a description/location, one hero is
"dispatched" to handle it and becomes unavailable, an incident has a severity
rating) into DQ-flavored names. Instead, ask what an actual Dragon Quest game does
for that concept first, and design around that — even where it means dropping a
brief-suggested field/shape entirely.

Concretely: DQ battles have the **whole living party fight together** — there is
no "one hero is sent to handle an encounter." There is no player-facing severity
label on an encounter, only per-monster/per-zone difficulty. If a required
technique (e.g. the injected strategy) doesn't have an obvious DQ-authentic hook,
find a real DQ mechanic that needs a swappable decision (e.g. which party member a
monster's attack targets, or turn order by speed) rather than forcing the brief's
suggested "dispatch strategy" to fit.

## XML doc comments: describe the domain, not the assignment

Write `<summary>` (and other XML doc) comments from inside the game's own domain
model — describe what a class/method *is* or *does* in this codebase. Never write
comments that narrate a design decision relative to the assignment brief, kernekrav
requirements, or "why this doesn't have field X" — that rationale belongs in
`docs/domain-model.md`/`PROGRESS.md`, not in a doc comment a reader sees with zero
assignment context.

Bad: `/// A monster encounter — there's nothing here to assign since the whole party fights together.`
Good: `/// A monster encounter on the overworld.`
