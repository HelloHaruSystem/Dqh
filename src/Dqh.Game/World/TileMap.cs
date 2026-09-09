namespace Dqh.Game.World;

/// <summary>Overworld tile data, with no rendering concerns of its own.</summary>
internal sealed class TileMap
{
    private readonly TileType[,] _tiles;

    public int Columns { get; }
    public int Rows { get; }

    /// <param name="columns">Grid width in tiles.</param>
    /// <param name="rows">Grid height in tiles.</param>
    public TileMap(int columns, int rows)
    {
        Columns = columns;
        Rows = rows;
        _tiles = new TileType[rows, columns];
    }

    /// <returns>The tile at the given column/row.</returns>
    public TileType GetTile(int column, int row) => _tiles[row, column];

    public void SetTile(int column, int row, TileType tile) => _tiles[row, column] = tile;
}
