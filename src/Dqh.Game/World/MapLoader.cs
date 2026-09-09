using System.Text.Json;

namespace Dqh.Game.World;

/// <summary>Loads a map's tile grid (CSV of <see cref="TileType"/> indices) and entity data (JSON) from <c>Assets/Maps</c>.</summary>
internal static class MapLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static (TileMap Map, MapEntities Entities) Load(string mapName)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "Assets", "Maps");
        var map = LoadTileMap(Path.Combine(directory, $"{mapName}.csv"));
        var entities = LoadEntities(Path.Combine(directory, $"{mapName}.json"));

        foreach (var decoration in entities.Decorations)
        {
            map.Block(decoration.Column, decoration.Row);
        }

        return (map, entities);
    }

    private static TileMap LoadTileMap(string path)
    {
        var lines = File.ReadAllLines(path).Where(line => line.Length > 0).ToArray();
        var rows = lines.Length;
        var columns = lines[0].Split(',').Length;
        var map = new TileMap(columns, rows);

        for (var row = 0; row < rows; row++)
        {
            var cells = lines[row].Split(',');
            for (var column = 0; column < columns; column++)
            {
                map.SetTile(column, row, (TileType)int.Parse(cells[column]));
            }
        }

        return map;
    }

    private static MapEntities LoadEntities(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<MapEntities>(json, JsonOptions) ?? new MapEntities();
    }
}
