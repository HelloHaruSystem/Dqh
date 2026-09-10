using Dqh.Game.Input;
using Dqh.Game.Presentation;
using Dqh.Game.Rendering;
using Dqh.Game.Settings;
using Dqh.Game.Timing;
using Dqh.Game.World;
using Raylib_cs;

namespace Dqh.Game;

/// <summary>
/// Builds the concrete <see cref="IInputSource"/>/<see cref="IWorldPresenter"/>/
/// <see cref="IClock"/> for headless or windowed mode and hands off to <see cref="GameLoop"/>.
/// The only place besides <c>Program.cs</c> that references Raylib directly.
/// </summary>
internal static class Bootstrap
{
    public static void RunHeadless(GameWorld world, PlayerMarker player)
    {
        IInputSource input = new ConsoleInputSource();
        IWorldPresenter presenter = new ConsoleWorldPresenter();
        IClock clock = new HeadlessClock();

        GameLoop.Run(world, player, input, presenter, clock);
    }

    public static void RunWindowed(GameWorld world, PlayerMarker player)
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(GridSettings.WindowWidth, GridSettings.WindowHeight, "DQH - overworld proof of concept");
        Raylib.SetWindowMinSize(GridSettings.MinWindowWidth, GridSettings.MinWindowHeight);
        Raylib.SetTargetFPS(GridSettings.TargetFps);

        // Texture loading needs the window/GL context to already exist, so these
        // are constructed here, not before InitWindow.
        var tileRenderer = new TexturedTileRenderer();
        var propRenderer = new PropRenderer();
        var npcRenderer = new NpcRenderer();
        var monsterRenderer = new MonsterRenderer();
        var heroAssetPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Characters", "hero.png");
        var playerRenderer = new SpriteActorRenderer(heroAssetPath, frameColumns: 2, frameRows: 3);

        IInputSource input = new RaylibInputSource();
        IWorldPresenter presenter = new RaylibWorldPresenter(tileRenderer, playerRenderer, propRenderer, npcRenderer, monsterRenderer);
        IClock clock = new RaylibClock();

        GameLoop.Run(world, player, input, presenter, clock);

        tileRenderer.Dispose();
        propRenderer.Dispose();
        npcRenderer.Dispose();
        monsterRenderer.Dispose();
        playerRenderer.Dispose();
        Raylib.CloseWindow();
    }
}
