namespace Dqh.Game.Input;

/// <summary>
/// Reads moves and the confirm action from stdin, one line at a time — a real
/// loop, not a pre-parsed script. Each line is parsed as a WASD-plus-'e'
/// sequence (so "dds e" queues two moves then a confirm); a blank line, EOF, or
/// "q"/"quit" requests quit. Fully pipeable for scripted testing:
/// `echo "dd\nss\nq" | dotnet run -- --headless`.
/// </summary>
internal sealed class ConsoleInputSource : IInputSource
{
    private readonly Queue<InputCommand> _pending = new();
    private bool _quitRequested;

    public bool IsQuitRequested => _quitRequested;

    public bool TryGetMove(float deltaSeconds, out int columnDelta, out int rowDelta)
    {
        if (Peek() is { IsConfirm: false } command)
        {
            _pending.Dequeue();
            columnDelta = command.ColumnDelta;
            rowDelta = command.RowDelta;
            return true;
        }

        columnDelta = 0;
        rowDelta = 0;
        return false;
    }

    public bool TryGetConfirm()
    {
        if (Peek() is { IsConfirm: true })
        {
            _pending.Dequeue();
            return true;
        }

        return false;
    }

    /// <summary>Reads another line (blocking) only once the queue runs dry; returns the next command without consuming it.</summary>
    private InputCommand? Peek()
    {
        while (_pending.Count == 0 && !_quitRequested)
        {
            var line = Console.ReadLine();

            if (line is null || line.Trim() is "" or "q" or "quit")
            {
                _quitRequested = true;
                break;
            }

            foreach (var command in InputScript.Parse(line))
            {
                _pending.Enqueue(command);
            }
        }

        return _pending.Count > 0 ? _pending.Peek() : null;
    }
}
