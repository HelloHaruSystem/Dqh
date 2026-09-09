using Dqh.Game;
using Dqh.Game.Input;
using Dqh.Game.Presentation;
using Dqh.Game.Rendering;
using Dqh.Game.Settings;
using Dqh.Game.Timing;
using Dqh.Game.World;
using Raylib_cs;

// Composition root: the only place that decides which concrete IInputSource/
// IWorldPresenter/IClock to use. GameLoop itself never knows Raylib exists,
// which is what makes headless mode possible.
var (map, entities) = MapLoader.Load("overworld");
var player = new PlayerMarker(map, entities.PlayerSpawn.Column, entities.PlayerSpawn.Row);

var headless = args.Any(a => a.Equals("--headless", StringComparison.OrdinalIgnoreCase));

if (headless)
{
    RunHeadless(map, entities.Decorations, player);
}
else
{
    RunWindowed(map, entities.Decorations, player);
}

static void RunHeadless(TileMap map, List<DecorationData> decorations, PlayerMarker player)
{
    IInputSource input = new ConsoleInputSource();
    IWorldPresenter presenter = new ConsoleWorldPresenter();
    IClock clock = new HeadlessClock();

    GameLoop.Run(map, decorations, player, input, presenter, clock);
}

static void RunWindowed(TileMap map, List<DecorationData> decorations, PlayerMarker player)
{
    Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
    Raylib.InitWindow(GridSettings.WindowWidth, GridSettings.WindowHeight, "DQH - overworld proof of concept");
    Raylib.SetWindowMinSize(GridSettings.MinWindowWidth, GridSettings.MinWindowHeight);
    Raylib.SetTargetFPS(GridSettings.TargetFps);

    // Texture loading needs the window/GL context to already exist, so these
    // are constructed here, not before InitWindow.
    var tileRenderer = new TexturedTileRenderer();
    var propRenderer = new PropRenderer();
    var heroAssetPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Characters", "hero.png");
    var playerRenderer = new SpriteActorRenderer(heroAssetPath, frameColumns: 2, frameRows: 3);

    IInputSource input = new RaylibInputSource();
    IWorldPresenter presenter = new RaylibWorldPresenter(tileRenderer, playerRenderer, propRenderer);
    IClock clock = new RaylibClock();

    GameLoop.Run(map, decorations, player, input, presenter, clock);

    tileRenderer.Dispose();
    propRenderer.Dispose();
    playerRenderer.Dispose();
    Raylib.CloseWindow();
}
