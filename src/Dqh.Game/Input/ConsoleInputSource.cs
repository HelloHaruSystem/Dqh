namespace Dqh.Game.Input;

/// <summary>
/// Reads moves from stdin, one line at a time — a real loop, not a pre-parsed
/// script. Each line is parsed as a WASD sequence (so "dd" queues two moves);
/// a blank line, EOF, or "q"/"quit" requests quit. Fully pipeable for scripted
/// testing: `echo "dd\nss\nq" | dotnet run -- --headless`.
/// </summary>
internal sealed class ConsoleInputSource : IInputSource
{
    private readonly Queue<(int ColumnDelta, int RowDelta)> _pending = new();
    private bool _quitRequested;

    public bool IsQuitRequested => _quitRequested;

    public bool TryGetMove(float deltaSeconds, out int columnDelta, out int rowDelta)
    {
        while (_pending.Count == 0 && !_quitRequested)
        {
            var line = Console.ReadLine();

            if (line is null || line.Trim() is "" or "q" or "quit")
            {
                _quitRequested = true;
                break;
            }

            foreach (var move in MoveScript.Parse(line))
            {
                _pending.Enqueue(move);
            }
        }

        if (_pending.Count == 0)
        {
            columnDelta = 0;
            rowDelta = 0;
            return false;
        }

        (columnDelta, rowDelta) = _pending.Dequeue();
        return true;
    }
}
