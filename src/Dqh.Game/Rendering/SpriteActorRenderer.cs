using System.Numerics;
using Raylib_cs;

namespace Dqh.Game.Rendering;

/// <summary>
/// Draws an actor from a texture. If the source is a sprite sheet, this always
/// draws the top-left frame (row 0, column 0) — direction/animation frame
/// selection isn't wired up yet, this just proves real art renders correctly.
/// </summary>
internal sealed class SpriteActorRenderer : IActorRenderer, IDisposable
{
    private readonly Texture2D _texture;
    private readonly Rectangle _frame;

    /// <param name="assetPath">Full path to the texture.</param>
    /// <param name="frameColumns">How many frames wide the sheet is (1 for a plain single-image sprite).</param>
    /// <param name="frameRows">How many frames tall the sheet is (1 for a plain single-image sprite).</param>
    public SpriteActorRenderer(string assetPath, int frameColumns = 1, int frameRows = 1)
    {
        _texture = Raylib.LoadTexture(assetPath);
        _frame = new Rectangle(0, 0, _texture.Width / frameColumns, _texture.Height / frameRows);
    }

    public void Draw(int column, int row, Camera camera, Viewport viewport)
    {
        var screenColumn = column - camera.StartColumn;
        var screenRow = row - camera.StartRow;
        var destination = new Rectangle(viewport.PixelX(screenColumn), viewport.PixelY(screenRow), viewport.TileSizePixels, viewport.TileSizePixels);
        Raylib.DrawTexturePro(_texture, _frame, destination, Vector2.Zero, 0f, Color.White);
    }

    public void Dispose() => Raylib.UnloadTexture(_texture);
}
