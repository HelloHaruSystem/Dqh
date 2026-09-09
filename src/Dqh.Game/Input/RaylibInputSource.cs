using Dqh.Game.Settings;
using Raylib_cs;

namespace Dqh.Game.Input;

/// <summary>
/// Reads arrow keys / WASD from the Raylib window. A tap moves once
/// immediately; holding a key repeats the move at a fixed interval (not every
/// frame — at 60fps that would slide the player 60 tiles/second).
/// </summary>
internal sealed class RaylibInputSource : IInputSource
{
    private float _timeSinceLastMove;

    public bool IsQuitRequested => Raylib.WindowShouldClose();

    public bool TryGetMove(float deltaSeconds, out int columnDelta, out int rowDelta)
    {
        if (TryGetDirection(isDown: false, out columnDelta, out rowDelta))
        {
            _timeSinceLastMove = 0f;
            return true;
        }

        if (!TryGetDirection(isDown: true, out columnDelta, out rowDelta))
        {
            _timeSinceLastMove = 0f;
            return false;
        }

        _timeSinceLastMove += deltaSeconds;
        if (_timeSinceLastMove < MovementSettings.StepIntervalSeconds) return false;

        _timeSinceLastMove = 0f;
        return true;
    }

    /// <param name="isDown">true to check "held down" (for repeat), false to check "just pressed" (for the initial tap).</param>
    private static bool TryGetDirection(bool isDown, out int columnDelta, out int rowDelta)
    {
        columnDelta = 0;
        rowDelta = 0;

        if (IsActive(KeyboardKey.Right, isDown) || IsActive(KeyboardKey.D, isDown)) columnDelta = 1;
        else if (IsActive(KeyboardKey.Left, isDown) || IsActive(KeyboardKey.A, isDown)) columnDelta = -1;
        else if (IsActive(KeyboardKey.Down, isDown) || IsActive(KeyboardKey.S, isDown)) rowDelta = 1;
        else if (IsActive(KeyboardKey.Up, isDown) || IsActive(KeyboardKey.W, isDown)) rowDelta = -1;
        else return false;

        return true;
    }

    private static bool IsActive(KeyboardKey key, bool isDown) =>
        isDown ? Raylib.IsKeyDown(key) : Raylib.IsKeyPressed(key);

    public bool TryGetConfirm() =>
        Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space) || Raylib.IsKeyPressed(KeyboardKey.E);
}
