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
    public static void Run(GameWorld world, PlayerMarker player, IInputSource input, IWorldPresenter presenter, IClock clock)
    {
        presenter.Present(world, player, clock.DeltaSeconds);

        while (!input.IsQuitRequested)
        {
            Update(world, player, input, clock.DeltaSeconds);
            presenter.Present(world, player, clock.DeltaSeconds);
        }
    }

    private static void Update(GameWorld world, PlayerMarker player, IInputSource input, float deltaSeconds)
    {
        player.Tick(deltaSeconds);
        world.Tick(deltaSeconds, player);

        // Movement is locked out for the duration of a map transition — the
        // fade itself is the only thing that should be happening on screen.
        if (world.IsTransitioning) return;

        if (input.TryGetMove(deltaSeconds, out var columnDelta, out var rowDelta))
        {
            player.Move(columnDelta, rowDelta);
            world.CheckPortal(player);
        }
    }
}
