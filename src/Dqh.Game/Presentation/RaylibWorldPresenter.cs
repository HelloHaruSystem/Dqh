using Dqh.Game.Rendering;
using Dqh.Game.World;
using Raylib_cs;

namespace Dqh.Game.Presentation;

/// <summary>Draws one frame to the Raylib window, delegating actual shape drawing to the tile/actor renderers.</summary>
internal sealed class RaylibWorldPresenter : IWorldPresenter
{
    private readonly ITileRenderer _tileRenderer;
    private readonly IActorRenderer _playerRenderer;

    public RaylibWorldPresenter(ITileRenderer tileRenderer, IActorRenderer playerRenderer)
    {
        _tileRenderer = tileRenderer;
        _playerRenderer = playerRenderer;
    }

    public void Present(TileMap map, PlayerMarker player)
    {
        var viewport = Viewport.Fit(map.Columns, map.Rows, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Palette.WindowBackground);
        _tileRenderer.Draw(map, viewport);
        _playerRenderer.Draw(player.Column, player.Row, viewport);
        Raylib.EndDrawing();
    }
}
