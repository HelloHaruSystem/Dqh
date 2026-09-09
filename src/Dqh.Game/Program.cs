using Dqh.Game.Rendering;
using Dqh.Game.Settings;
using Dqh.Game.World;
using Raylib_cs;

// Composition root: this is the only place concrete renderer types are chosen.
// Everything else depends on ITileRenderer / IActorRenderer, so swapping in a
// bitmap-based renderer later only means changing the two lines below.
var map = new TileMap(GridSettings.Columns, GridSettings.Rows);
var player = new PlayerMarker(map, GridSettings.Columns / 2, GridSettings.Rows / 2);

ITileRenderer tileRenderer = new CheckerboardTileRenderer();
IActorRenderer playerRenderer = new RectangleActorRenderer(Palette.Player);

Raylib.InitWindow(GridSettings.WindowWidth, GridSettings.WindowHeight, "DQH - overworld proof of concept");
Raylib.SetTargetFPS(GridSettings.TargetFps);

while (!Raylib.WindowShouldClose())
{
    if (Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressed(KeyboardKey.D)) player.Move(1, 0);
    if (Raylib.IsKeyPressed(KeyboardKey.Left) || Raylib.IsKeyPressed(KeyboardKey.A)) player.Move(-1, 0);
    if (Raylib.IsKeyPressed(KeyboardKey.Down) || Raylib.IsKeyPressed(KeyboardKey.S)) player.Move(0, 1);
    if (Raylib.IsKeyPressed(KeyboardKey.Up) || Raylib.IsKeyPressed(KeyboardKey.W)) player.Move(0, -1);

    Raylib.BeginDrawing();
    Raylib.ClearBackground(Palette.WindowBackground);
    tileRenderer.Draw(map);
    playerRenderer.Draw(player.Column, player.Row);
    Raylib.EndDrawing();
}

Raylib.CloseWindow();
