using System.Numerics;
using Dqh.Domain.Combatants.Monsters;
using Raylib_cs;

namespace Dqh.Game.Rendering;

/// <summary>Draws a monster's real texture from <c>Assets/Monsters</c>, looked up by kind.</summary>
internal sealed class MonsterRenderer : IDisposable
{
    private readonly Dictionary<MonsterKind, Texture2D> _textures = [];
    private readonly string _directory = Path.Combine(AppContext.BaseDirectory, "Assets", "Monsters");

    public void Draw(MonsterKind kind, int x, int y, int sizePixels)
    {
        var texture = GetTexture(kind);
        var source = new Rectangle(0, 0, texture.Width, texture.Height);
        var destination = new Rectangle(x, y, sizePixels, sizePixels);
        Raylib.DrawTexturePro(texture, source, destination, Vector2.Zero, 0f, Color.White);
    }

    public void Dispose()
    {
        foreach (var texture in _textures.Values)
        {
            Raylib.UnloadTexture(texture);
        }
    }

    private Texture2D GetTexture(MonsterKind kind)
    {
        if (!_textures.TryGetValue(kind, out var texture))
        {
            texture = Raylib.LoadTexture(Path.Combine(_directory, $"{FileNameFor(kind)}.png"));
            _textures[kind] = texture;
        }

        return texture;
    }

    /// <summary>Asset filenames are snake_case; <see cref="MonsterKind"/> names aren't (e.g. MetalSlime → metal_slime.png).</summary>
    private static string FileNameFor(MonsterKind kind) => kind switch
    {
        MonsterKind.Slime => "slime",
        MonsterKind.Dracky => "dracky",
        MonsterKind.Ghost => "ghost",
        MonsterKind.MetalSlime => "metal_slime",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "No sprite for this monster kind."),
    };
}
