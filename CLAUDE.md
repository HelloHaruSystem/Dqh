# CLAUDE.md

## Git

The user handles all non-read-only git operations themselves (staging, committing,
branching, pushing, etc.). Only use read-only git commands (`status`, `diff`, `log`,
`branch` without creating/deleting, etc.) — never `add`, `commit`, `push`,
`checkout`/`switch`, `branch -d`, `merge`, `rebase`, or similar, even if asked to
"save" or "finish up" work. Leave the working tree as-is and let the user decide
when/how to commit.
