namespace Dqh.Game.World;

/// <summary>A cursor over a list of on-screen options, wrapping at either end.</summary>
internal sealed class BattleMenu
{
    public IReadOnlyList<string> Options { get; }
    public int SelectedIndex { get; private set; }

    public BattleMenu(IReadOnlyList<string> options)
    {
        if (options.Count == 0)
            throw new ArgumentException("A menu needs at least one option.", nameof(options));

        Options = options;
    }

    public void MoveCursor(int direction) =>
        SelectedIndex = ((SelectedIndex + direction) % Options.Count + Options.Count) % Options.Count;
}
