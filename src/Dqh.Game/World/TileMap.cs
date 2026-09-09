namespace Dqh.Game.World;

/// <summary>Overworld tile data, with no rendering concerns of its own.</summary>
internal sealed class TileMap
{
    private readonly TileType[,] _tiles;
    private readonly HashSet<(int Column, int Row)> _blockedPositions = [];

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

    /// <returns>Whether the given column/row can be walked onto — its own terrain, and nothing solid placed on top of it.</returns>
    public bool IsWalkable(int column, int row) => GetTile(column, row).IsWalkable() && !_blockedPositions.Contains((column, row));

    /// <summary>Marks a cell as occupied by something solid (e.g. a decoration) regardless of its terrain.</summary>
    public void Block(int column, int row) => _blockedPositions.Add((column, row));
}
