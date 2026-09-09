using Dqh.Game.Input;
using Dqh.Game.Presentation;
using Dqh.Game.World;

namespace Dqh.Game;

/// <summary>
/// The tick loop itself, independent of Raylib. Identical for the windowed and
/// headless modes — only the <see cref="IInputSource"/>/<see cref="IWorldPresenter"/>
/// passed in differ.
/// </summary>
internal static class GameLoop
{
    public static void Run(TileMap map, PlayerMarker player, IInputSource input, IWorldPresenter presenter)
    {
        presenter.Present(map, player);

        while (!input.IsQuitRequested)
        {
            if (input.TryGetMove(out var columnDelta, out var rowDelta))
            {
                player.Move(columnDelta, rowDelta);
            }

            presenter.Present(map, player);
        }
    }
}
