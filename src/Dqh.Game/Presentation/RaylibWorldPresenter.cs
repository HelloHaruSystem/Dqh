using Dqh.Game.Rendering;
using Dqh.Game.Settings;
using Dqh.Game.World;
using Raylib_cs;

namespace Dqh.Game.Presentation;

/// <summary>Draws one frame to the Raylib window, delegating actual shape drawing to the tile/actor renderers.</summary>
internal sealed class RaylibWorldPresenter : IWorldPresenter
{
    private readonly ITileRenderer _tileRenderer;
    private readonly IActorRenderer _playerRenderer;
    private readonly PropRenderer _propRenderer;
    private readonly NpcRenderer _npcRenderer;

    public RaylibWorldPresenter(ITileRenderer tileRenderer, IActorRenderer playerRenderer, PropRenderer propRenderer, NpcRenderer npcRenderer)
    {
        _tileRenderer = tileRenderer;
        _playerRenderer = playerRenderer;
        _propRenderer = propRenderer;
        _npcRenderer = npcRenderer;
    }

    public void Present(TileMap map, IReadOnlyList<DecorationData> decorations, IReadOnlyList<NpcData> npcs, PlayerMarker player, float deltaSeconds)
    {
        var camera = Camera.Follow(map, player.Column, player.Row, GridSettings.CameraColumns, GridSettings.CameraRows);
        var viewport = Viewport.Fit(camera.VisibleColumns, camera.VisibleRows, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Palette.WindowBackground);
        _tileRenderer.Draw(map, camera, viewport);
        _propRenderer.Draw(decorations, camera, viewport);
        _npcRenderer.Draw(npcs, camera, viewport);
        _playerRenderer.Draw(player.Column, player.Row, player.Facing, player.WalkFrame, camera, viewport);
        Raylib.EndDrawing();
    }
}
