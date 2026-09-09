using System.Numerics;
using Dqh.Game.World;
using Raylib_cs;

namespace Dqh.Game.Rendering;

/// <summary>
/// Draws an actor from a directional walk-cycle sheet: one row per facing
/// (down, up, left — right reuses the left row, mirrored) and one column per
/// walk-cycle frame.
/// </summary>
internal sealed class SpriteActorRenderer : IActorRenderer, IDisposable
{
    private readonly Texture2D _texture;
    private readonly int _frameWidth;
    private readonly int _frameHeight;

    /// <param name="assetPath">Full path to the texture.</param>
    /// <param name="frameColumns">How many walk-cycle frames wide the sheet is (1 for a plain single-image sprite).</param>
    /// <param name="frameRows">How many facings tall the sheet is (1 for a plain single-image sprite).</param>
    public SpriteActorRenderer(string assetPath, int frameColumns = 1, int frameRows = 1)
    {
        _texture = Raylib.LoadTexture(assetPath);
        _frameWidth = _texture.Width / frameColumns;
        _frameHeight = _texture.Height / frameRows;
    }

    public void Draw(int column, int row, Direction facing, int walkFrame, Camera camera, Viewport viewport)
    {
        var screenColumn = column - camera.StartColumn;
        var screenRow = row - camera.StartRow;
        var destination = new Rectangle(viewport.PixelX(screenColumn), viewport.PixelY(screenRow), viewport.TileSizePixels, viewport.TileSizePixels);

        var spriteRow = facing switch
        {
            Direction.Down => 0,
            Direction.Up => 1,
            _ => 2, // Left and Right share the left-facing row; Right is mirrored below.
        };
        var mirrored = facing == Direction.Right;
        var source = new Rectangle(walkFrame * _frameWidth, spriteRow * _frameHeight, mirrored ? -_frameWidth : _frameWidth, _frameHeight);

        Raylib.DrawTexturePro(_texture, source, destination, Vector2.Zero, 0f, Color.White);
    }

    public void Dispose() => Raylib.UnloadTexture(_texture);
}
