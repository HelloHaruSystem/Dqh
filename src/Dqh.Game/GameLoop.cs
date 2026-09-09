using Dqh.Game.Input;
using Dqh.Game.Presentation;
using Dqh.Game.Timing;
using Dqh.Game.World;

namespace Dqh.Game;

/// <summary>
/// The tick loop itself, independent of Raylib. Identical for the windowed and
/// headless modes — only the <see cref="IInputSource"/>/<see cref="IWorldPresenter"/>/
/// <see cref="IClock"/> passed in differ.
/// </summary>
internal static class GameLoop
{
    public static void Run(TileMap map, IReadOnlyList<DecorationData> decorations, PlayerMarker player, IInputSource input, IWorldPresenter presenter, IClock clock)
    {
        presenter.Present(map, decorations, player, clock.DeltaSeconds);

        while (!input.IsQuitRequested)
        {
            Update(player, input, clock.DeltaSeconds);
            presenter.Present(map, decorations, player, clock.DeltaSeconds);
        }
    }

    private static void Update(PlayerMarker player, IInputSource input, float deltaSeconds)
    {
        if (input.TryGetMove(deltaSeconds, out var columnDelta, out var rowDelta))
        {
            player.Move(columnDelta, rowDelta);
        }
    }
}
