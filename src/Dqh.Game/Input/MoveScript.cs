namespace Dqh.Game.Input;

/// <summary>Parses a WASD-style command string (e.g. "ddssaw") into grid moves, for headless scripting.</summary>
internal static class MoveScript
{
    public static IEnumerable<(int ColumnDelta, int RowDelta)> Parse(string commands)
    {
        foreach (var c in commands)
        {
            var move = char.ToLowerInvariant(c) switch
            {
                'd' => (1, 0),
                'a' => (-1, 0),
                's' => (0, 1),
                'w' => (0, -1),
                _ => ((int, int)?)null,
            };

            if (move is { } delta) yield return delta;
        }
    }
}
