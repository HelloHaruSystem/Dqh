using System.Numerics;
using Dqh.Game.World;
using Raylib_cs;

namespace Dqh.Game.Rendering;

/// <summary>Draws static map NPCs from their real textures in <c>Assets/Characters</c>, looked up by NPC id.</summary>
internal sealed class NpcRenderer : IDisposable
{
    private readonly Dictionary<string, Texture2D> _textures = [];
    private readonly string _directory = Path.Combine(AppContext.BaseDirectory, "Assets", "Characters");

    public void Draw(IReadOnlyList<NpcData> npcs, Camera camera, Viewport viewport)
    {
        foreach (var npc in npcs)
        {
            var screenColumn = npc.Column - camera.StartColumn;
            var screenRow = npc.Row - camera.StartRow;
            if (screenColumn < 0 || screenColumn >= camera.VisibleColumns || screenRow < 0 || screenRow >= camera.VisibleRows)
            {
                continue;
            }

            var texture = GetTexture(npc.Id);
            var source = new Rectangle(0, 0, texture.Width, texture.Height);
            var destination = new Rectangle(viewport.PixelX(screenColumn), viewport.PixelY(screenRow), viewport.TileSizePixels, viewport.TileSizePixels);
            Raylib.DrawTexturePro(texture, source, destination, Vector2.Zero, 0f, Color.White);
        }
    }

    public void Dispose()
    {
        foreach (var texture in _textures.Values)
        {
            Raylib.UnloadTexture(texture);
        }
    }

    private Texture2D GetTexture(string id)
    {
        if (!_textures.TryGetValue(id, out var texture))
        {
            texture = Raylib.LoadTexture(Path.Combine(_directory, $"{id}.png"));
            _textures[id] = texture;
        }

        return texture;
    }
}
