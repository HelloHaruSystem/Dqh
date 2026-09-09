using Raylib_cs;

namespace Dqh.Game.Rendering;

/// <summary>Placeholder <see cref="IActorRenderer"/>: a flat-colored rectangle, no sprites.</summary>
internal sealed class RectangleActorRenderer : IActorRenderer
{
    private readonly Color _color;

    public RectangleActorRenderer(Color color)
    {
        _color = color;
    }

    public void Draw(int column, int row, Viewport viewport)
    {
        Raylib.DrawRectangle(
            viewport.PixelX(column),
            viewport.PixelY(row),
            viewport.TileSizePixels,
            viewport.TileSizePixels,
            _color);
    }
}
