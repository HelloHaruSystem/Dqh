using Dqh.Game.World;

namespace Dqh.Game.Rendering;

/// <summary>
/// Which tiles are visible on screen — a fixed-size window centered on the
/// player, clamped so it never scrolls past the map's edges. Recomputed every
/// frame from the player's current position.
/// </summary>
internal readonly record struct Camera(int StartColumn, int StartRow, int VisibleColumns, int VisibleRows)
{
    public static Camera Follow(TileMap map, int playerColumn, int playerRow, int visibleColumns, int visibleRows)
    {
        var columns = Math.Min(visibleColumns, map.Columns);
        var rows = Math.Min(visibleRows, map.Rows);

        var startColumn = Math.Clamp(playerColumn - columns / 2, 0, map.Columns - columns);
        var startRow = Math.Clamp(playerRow - rows / 2, 0, map.Rows - rows);

        return new Camera(startColumn, startRow, columns, rows);
    }
}
