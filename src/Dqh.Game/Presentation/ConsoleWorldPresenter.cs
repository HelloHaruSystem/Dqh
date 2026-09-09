using Dqh.Game.World;

namespace Dqh.Game.Presentation;

/// <summary>Prints an ASCII dump of the world each tick. No window, no Raylib dependency — for headless/scripted runs.</summary>
internal sealed class ConsoleWorldPresenter : IWorldPresenter
{
    private const char PlayerSymbol = '@';
    private const char GrassSymbol = '.';
    private const char EncounterZoneSymbol = '"';

    public void Present(TileMap map, PlayerMarker player)
    {
        for (var row = 0; row < map.Rows; row++)
        {
            var line = new char[map.Columns];
            for (var col = 0; col < map.Columns; col++)
            {
                line[col] = col == player.Column && row == player.Row
                    ? PlayerSymbol
                    : SymbolFor(map.GetTile(col, row));
            }

            Console.WriteLine(new string(line));
        }

        Console.WriteLine($"Player at ({player.Column}, {player.Row})");
        Console.WriteLine();
    }

    private static char SymbolFor(TileType tile) => tile switch
    {
        TileType.EncounterZone => EncounterZoneSymbol,
        _ => GrassSymbol,
    };
}
