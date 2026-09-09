namespace Dqh.Game.World;

/// <summary>Player's grid position. Movement only, no rendering.</summary>
internal sealed class PlayerMarker
{
    private TileMap _map;

    public int Column { get; private set; }
    public int Row { get; private set; }

    public PlayerMarker(TileMap map, int startColumn, int startRow)
    {
        _map = map;
        Column = startColumn;
        Row = startRow;
    }

    /// <summary>Moves by one tile if the destination is within bounds and walkable.</summary>
    /// <param name="columnDelta">-1, 0, or 1.</param>
    /// <param name="rowDelta">-1, 0, or 1.</param>
    public void Move(int columnDelta, int rowDelta)
    {
        var newColumn = Column + columnDelta;
        var newRow = Row + rowDelta;

        if (newColumn < 0 || newColumn >= _map.Columns) return;
        if (newRow < 0 || newRow >= _map.Rows) return;
        if (!_map.GetTile(newColumn, newRow).IsWalkable()) return;

        Column = newColumn;
        Row = newRow;
    }

    /// <summary>Relocates to a different map — e.g. walking through a door into another scene.</summary>
    public void WarpTo(TileMap map, int column, int row)
    {
        _map = map;
        Column = column;
        Row = row;
    }
}
