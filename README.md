# DQH

## Running

Build everything:

```
dotnet build
```

Run the game in the Raylib window (move with arrow keys / WASD, close the window to quit):

```
dotnet run --project src/Dqh.Game
```

Run it headless instead — no window, prints an ASCII grid to the console each tick.
Type moves one line at a time (e.g. `d`, `ss`, `wa`); a blank line, EOF, or
`q`/`quit` exits:

```
dotnet run --project src/Dqh.Game -- --headless
```

Run the domain test suite:

```
dotnet test
```
