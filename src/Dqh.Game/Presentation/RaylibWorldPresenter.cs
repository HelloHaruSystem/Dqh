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

    public void Present(GameWorld world, PlayerMarker player, float deltaSeconds)
    {
        var camera = Camera.Follow(world.Map, player.Column, player.Row, GridSettings.CameraColumns, GridSettings.CameraRows);
        var viewport = Viewport.Fit(camera.VisibleColumns, camera.VisibleRows, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Palette.WindowBackground);
        _tileRenderer.Draw(world.Map, camera, viewport);
        _propRenderer.Draw(world.Decorations, camera, viewport);
        _npcRenderer.Draw(world.Npcs, camera, viewport);
        _playerRenderer.Draw(player.Column, player.Row, player.Facing, player.WalkFrame, camera, viewport);

        if (world.TransitionFade > 0f)
        {
            var alpha = (byte)Math.Clamp(world.TransitionFade * 255f, 0f, 255f);
            Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), new Color((byte)0, (byte)0, (byte)0, alpha));
        }

        Raylib.EndDrawing();
    }
}
