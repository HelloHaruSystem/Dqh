namespace Dqh.Game.Input;

/// <summary>One parsed token from a headless input script: either a grid move or the confirm/interact action.</summary>
internal readonly record struct InputCommand(bool IsConfirm, int ColumnDelta = 0, int RowDelta = 0)
{
    public static InputCommand Move(int columnDelta, int rowDelta) => new(IsConfirm: false, columnDelta, rowDelta);
    public static readonly InputCommand Confirm = new(IsConfirm: true);
}

/// <summary>
/// Parses a WASD-plus-confirm command string (e.g. "ddsse") into input commands,
/// for headless scripting. 'e' is the confirm/interact action.
/// </summary>
internal static class InputScript
{
    public static IEnumerable<InputCommand> Parse(string commands)
    {
        foreach (var c in commands)
        {
            var command = char.ToLowerInvariant(c) switch
            {
                'd' => InputCommand.Move(1, 0),
                'a' => InputCommand.Move(-1, 0),
                's' => InputCommand.Move(0, 1),
                'w' => InputCommand.Move(0, -1),
                'e' => InputCommand.Confirm,
                _ => (InputCommand?)null,
            };

            if (command is { } value) yield return value;
        }
    }
}
