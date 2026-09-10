using Dqh.Domain.Battles;
using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Encounters;
using Dqh.Domain.Party;
using Dqh.Domain.Strategies;
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
var party = new Party(new RandomTargetStrategy());
party.Register(new Hero("Hero"));
party.Register(new Warrior("Warrior"));
party.Register(new Mage("Mage"));
party.Register(new Priest("Priest"));
party.Register(new Ranger("Ranger"));

var world = new GameWorld("overworld", party, new RandomEncounterGenerator(), new StandardBattleResolver());
var player = new PlayerMarker(world.Map, world.PlayerSpawn.Column, world.PlayerSpawn.Row);

var headless = args.Any(a => a.Equals("--headless", StringComparison.OrdinalIgnoreCase));

if (headless)
{
    RunHeadless(world, player);
}
else
{
    RunWindowed(world, player);
}

static void RunHeadless(GameWorld world, PlayerMarker player)
{
    IInputSource input = new ConsoleInputSource();
    IWorldPresenter presenter = new ConsoleWorldPresenter();
    IClock clock = new HeadlessClock();

    GameLoop.Run(world, player, input, presenter, clock);
}

static void RunWindowed(GameWorld world, PlayerMarker player)
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
