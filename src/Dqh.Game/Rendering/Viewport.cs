namespace Dqh.Game.Rendering;

/// <summary>
/// Maps grid cells to pixels for the current window size: square tiles, scaled to
/// fit, centered (letterboxed) in whichever dimension has slack. Recomputed every
/// frame from the actual window size, so resizing the window rescales the grid
/// instead of clipping it.
/// </summary>
internal readonly record struct Viewport(int TileSizePixels, int OffsetX, int OffsetY)
{
    private const int MinTileSizePixels = 1;

    public static Viewport Fit(int columns, int rows, int windowWidth, int windowHeight)
    {
        var tileSize = Math.Max(MinTileSizePixels, Math.Min(windowWidth / columns, windowHeight / rows));
        var offsetX = (windowWidth - tileSize * columns) / 2;
        var offsetY = (windowHeight - tileSize * rows) / 2;
        return new Viewport(tileSize, offsetX, offsetY);
    }

    public int PixelX(int column) => OffsetX + column * TileSizePixels;
    public int PixelY(int row) => OffsetY + row * TileSizePixels;
}
