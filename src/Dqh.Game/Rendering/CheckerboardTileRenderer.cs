using Dqh.Game.Settings;
using Dqh.Game.World;
using Raylib_cs;

namespace Dqh.Game.Rendering;

/// <summary>Placeholder <see cref="ITileRenderer"/>: flat-colored tiles, no bitmaps.</summary>
internal sealed class CheckerboardTileRenderer : ITileRenderer
{
    public void Draw(TileMap map)
    {
        for (var row = 0; row < map.Rows; row++)
        {
            for (var col = 0; col < map.Columns; col++)
            {
                var color = ColorFor(map.GetTile(col, row), row, col);
                Raylib.DrawRectangle(
                    col * GridSettings.TileSizePixels,
                    row * GridSettings.TileSizePixels,
                    GridSettings.TileSizePixels,
                    GridSettings.TileSizePixels,
                    color);
            }
        }
    }

    private static Color ColorFor(TileType tile, int row, int col)
    {
        if (tile == TileType.EncounterZone) return Palette.EncounterZone;
        var isDark = (row + col) % 2 == 0;
        return isDark ? Palette.GrassDark : Palette.GrassLight;
    }
}
