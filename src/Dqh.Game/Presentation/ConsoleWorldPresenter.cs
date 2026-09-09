using Dqh.Game.World;

namespace Dqh.Game.Presentation;

/// <summary>Prints an ASCII dump of the world each tick. No window, no Raylib dependency — for headless/scripted runs.</summary>
internal sealed class ConsoleWorldPresenter : IWorldPresenter
{
    private const char PlayerSymbol = '@';

    public void Present(TileMap map, IReadOnlyList<DecorationData> decorations, IReadOnlyList<NpcData> npcs, PlayerMarker player, float deltaSeconds)
    {
        var occupantSymbols = decorations.ToDictionary(d => (d.Column, d.Row), d => SymbolFor(d.Id));
        foreach (var npc in npcs)
        {
            occupantSymbols[(npc.Column, npc.Row)] = SymbolFor(npc.Id);
        }

        for (var row = 0; row < map.Rows; row++)
        {
            var line = new char[map.Columns];
            for (var col = 0; col < map.Columns; col++)
            {
                line[col] = col == player.Column && row == player.Row
                    ? PlayerSymbol
                    : occupantSymbols.TryGetValue((col, row), out var occupantSymbol)
                        ? occupantSymbol
                        : SymbolFor(map.GetTile(col, row));
            }

            Console.WriteLine(new string(line));
        }

        Console.WriteLine($"Player at ({player.Column}, {player.Row})");
        Console.WriteLine();
    }

    private static char SymbolFor(TileType tile) => tile switch
    {
        TileType.Grass => '.',
        TileType.EncounterZone => '"',
        TileType.Mountain => '^',
        TileType.River => '~',
        TileType.Path => '=',
        TileType.Door => 'D',
        TileType.Floor => '_',
        TileType.Dock => '%',
        TileType.Roof => 'A',
        TileType.Wall => '#',
        TileType.BarCounter => 'C',
        TileType.BedHead => 'h',
        TileType.BedFoot => 'f',
        TileType.Table => 'T',
        _ => '?',
    };

    private static char SymbolFor(string decorationId) => decorationId switch
    {
        "sign" => 'S',
        "innkeeper" => 'I',
        "dock_worker" => 'W',
        _ => '!',
    };
}
