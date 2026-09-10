# DQH

A simple RPG demo in C#/.NET.

## Dependencies

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Raylib-cs](https://www.nuget.org/packages/Raylib-cs) for windowed rendering — restored automatically via NuGet, nothing to install by hand

## Running

```
dotnet build
dotnet run --project src/Dqh.Game                  # windowed: arrow keys / WASD, close the window to quit
dotnet run --project src/Dqh.Game -- --headless     # headless: prints an ASCII grid to the console each tick
dotnet test                                         # domain test suite
```

### Headless controls

Each line you type is read as a sequence of one-letter commands, applied in
order: `w`/`a`/`s`/`d` move one tile, `e` confirms/interacts. So `d` moves
right once, `ss` moves down twice, `wa` moves up then left. A blank line,
EOF, or `q`/`quit` exits.
