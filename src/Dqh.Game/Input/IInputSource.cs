namespace Dqh.Game.Input;

/// <summary>Where movement commands come from. Swappable so the loop doesn't care whether it's a keyboard, a script, or stdin.</summary>
internal interface IInputSource
{
    bool IsQuitRequested { get; }

    /// <param name="deltaSeconds">Time elapsed since the last tick — needed to pace key-repeat while a key is held.</param>
    /// <returns>true if a move was produced this tick.</returns>
    bool TryGetMove(float deltaSeconds, out int columnDelta, out int rowDelta);

    /// <returns>true if the confirm/interact action (advance or dismiss dialogue) was pressed this tick.</returns>
    bool TryGetConfirm();
}
