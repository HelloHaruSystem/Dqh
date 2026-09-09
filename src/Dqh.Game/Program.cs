using Dqh.Game;
using Dqh.Game.Input;
using Dqh.Game.Presentation;
using Dqh.Game.Rendering;
using Dqh.Game.Settings;
using Dqh.Game.World;
using Raylib_cs;

// Composition root: the only place that decides which concrete IInputSource/
// IWorldPresenter to use. GameLoop itself never knows Raylib exists, which is
// what makes headless mode possible: `dotnet run -- --headless` runs the exact
// same tick loop with no window, reading moves from stdin one line at a time
// instead of the keyboard, and dumping each frame as ASCII to the console.
var map = new TileMap(GridSettings.Columns, GridSettings.Rows);
var player = new PlayerMarker(map, GridSettings.Columns / 2, GridSettings.Rows / 2);

var headless = args.Any(a => a.Equals("--headless", StringComparison.OrdinalIgnoreCase));

if (headless)
{
    IInputSource input = new ConsoleInputSource();
    IWorldPresenter presenter = new ConsoleWorldPresenter();

    GameLoop.Run(map, player, input, presenter);
}
else
{
    Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
    Raylib.InitWindow(GridSettings.WindowWidth, GridSettings.WindowHeight, "DQH - overworld proof of concept");
    Raylib.SetWindowMinSize(GridSettings.MinWindowWidth, GridSettings.MinWindowHeight);
    Raylib.SetTargetFPS(GridSettings.TargetFps);

    IInputSource input = new RaylibInputSource();
    IWorldPresenter presenter = new RaylibWorldPresenter(new CheckerboardTileRenderer(), new RectangleActorRenderer(Palette.Player));

    GameLoop.Run(map, player, input, presenter);

    Raylib.CloseWindow();
}
