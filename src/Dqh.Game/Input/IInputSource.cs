namespace Dqh.Game.Input;

/// <summary>Where movement commands come from. Swappable so the loop doesn't care whether it's a keyboard, a script, or stdin.</summary>
internal interface IInputSource
{
    bool IsQuitRequested { get; }

    /// <returns>true if a move was produced this tick.</returns>
    bool TryGetMove(out int columnDelta, out int rowDelta);
}
