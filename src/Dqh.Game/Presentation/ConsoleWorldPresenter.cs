using Dqh.Game.World;

namespace Dqh.Game.Presentation;

/// <summary>Prints an ASCII dump of the world each tick. No window, no Raylib dependency — for headless/scripted runs.</summary>
internal sealed class ConsoleWorldPresenter : IWorldPresenter
{
    private const char PlayerSymbol = '@';

    public void Present(GameWorld world, PlayerMarker player, float deltaSeconds)
    {
        if (world.IsShowingWelcome)
        {
            Console.WriteLine("=== Welcome to DQH ===");
            Console.WriteLine("[e to begin]");
            Console.WriteLine();
            return;
        }

        var occupantSymbols = world.Decorations.ToDictionary(d => (d.Column, d.Row), d => SymbolFor(d.Id));
        foreach (var npc in world.Npcs)
        {
            occupantSymbols[(npc.Column, npc.Row)] = SymbolFor(npc.Id);
        }

        for (var row = 0; row < world.Map.Rows; row++)
        {
            var line = new char[world.Map.Columns];
            for (var col = 0; col < world.Map.Columns; col++)
            {
                line[col] = col == player.Column && row == player.Row
                    ? PlayerSymbol
                    : occupantSymbols.TryGetValue((col, row), out var occupantSymbol)
                        ? occupantSymbol
                        : SymbolFor(world.Map.GetTile(col, row));
            }

            Console.WriteLine(new string(line));
        }

        Console.WriteLine($"Player at ({player.Column}, {player.Row})");

        if (world.ActiveDialogueLine is { } activeLine)
        {
            Console.WriteLine($"> {activeLine} [e to continue]");
        }
        else if (world.IsFacingInteractable(player))
        {
            Console.WriteLine("[facing something — e to chat]");
        }

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
