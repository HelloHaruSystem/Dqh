using Dqh.Game.Settings;
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

    public void Draw(int column, int row)
    {
        Raylib.DrawRectangle(
            column * GridSettings.TileSizePixels,
            row * GridSettings.TileSizePixels,
            GridSettings.TileSizePixels,
            GridSettings.TileSizePixels,
            _color);
    }
}
