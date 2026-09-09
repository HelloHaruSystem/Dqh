using Raylib_cs;

namespace Dqh.Game.Input;

/// <summary>Reads arrow keys / WASD from the Raylib window.</summary>
internal sealed class RaylibInputSource : IInputSource
{
    public bool IsQuitRequested => Raylib.WindowShouldClose();

    public bool TryGetMove(out int columnDelta, out int rowDelta)
    {
        columnDelta = 0;
        rowDelta = 0;

        if (Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressed(KeyboardKey.D)) columnDelta = 1;
        else if (Raylib.IsKeyPressed(KeyboardKey.Left) || Raylib.IsKeyPressed(KeyboardKey.A)) columnDelta = -1;
        else if (Raylib.IsKeyPressed(KeyboardKey.Down) || Raylib.IsKeyPressed(KeyboardKey.S)) rowDelta = 1;
        else if (Raylib.IsKeyPressed(KeyboardKey.Up) || Raylib.IsKeyPressed(KeyboardKey.W)) rowDelta = -1;
        else return false;

        return true;
    }
}
