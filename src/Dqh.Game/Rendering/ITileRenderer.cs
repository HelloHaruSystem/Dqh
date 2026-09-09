using Dqh.Game.World;

namespace Dqh.Game.Rendering;

/// <summary>Draws a <see cref="TileMap"/>. Swappable so a bitmap/tileset renderer can replace the color-block one later.</summary>
internal interface ITileRenderer
{
    void Draw(TileMap map, Viewport viewport);
}
