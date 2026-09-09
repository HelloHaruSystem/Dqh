using System.Numerics;
using System.Text;
using Dqh.Game.World;
using Raylib_cs;

namespace Dqh.Game.Rendering;

/// <summary>Draws each tile from its real texture in <c>Assets/Tiles</c>, looked up by the tile type's name.</summary>
internal sealed class TexturedTileRenderer : ITileRenderer, IDisposable
{
    private readonly Dictionary<TileType, Texture2D> _textures = [];

    public TexturedTileRenderer()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "Assets", "Tiles");
        foreach (var tile in Enum.GetValues<TileType>())
        {
            var path = Path.Combine(directory, $"{ToSnakeCase(tile)}.png");
            _textures[tile] = Raylib.LoadTexture(path);
        }
    }

    public void Draw(TileMap map, Camera camera, Viewport viewport)
    {
        for (var screenRow = 0; screenRow < camera.VisibleRows; screenRow++)
        {
            for (var screenColumn = 0; screenColumn < camera.VisibleColumns; screenColumn++)
            {
                var texture = _textures[map.GetTile(camera.StartColumn + screenColumn, camera.StartRow + screenRow)];
                var source = new Rectangle(0, 0, texture.Width, texture.Height);
                var destination = new Rectangle(viewport.PixelX(screenColumn), viewport.PixelY(screenRow), viewport.TileSizePixels, viewport.TileSizePixels);
                Raylib.DrawTexturePro(texture, source, destination, Vector2.Zero, 0f, Color.White);
            }
        }
    }

    public void Dispose()
    {
        foreach (var texture in _textures.Values)
        {
            Raylib.UnloadTexture(texture);
        }
    }

    private static string ToSnakeCase(TileType tile)
    {
        var name = tile.ToString();
        var builder = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i])) builder.Append('_');
            builder.Append(char.ToLowerInvariant(name[i]));
        }
        return builder.ToString();
    }
}
