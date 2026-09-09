using Dqh.Game.Settings;

namespace Dqh.Game.World;

/// <summary>Player's grid position, facing, and walk-cycle frame.</summary>
internal sealed class PlayerMarker
{
    private TileMap _map;
    private float _timeSinceLastStep;

    public int Column { get; private set; }
    public int Row { get; private set; }
    public Direction Facing { get; private set; } = Direction.Down;
    public int WalkFrame { get; private set; }

    public PlayerMarker(TileMap map, int startColumn, int startRow)
    {
        _map = map;
        Column = startColumn;
        Row = startRow;
    }

    /// <summary>Advances the idle timer, resetting to a standing pose once it's been a while since the last step.</summary>
    public void Tick(float deltaSeconds)
    {
        _timeSinceLastStep += deltaSeconds;
        if (_timeSinceLastStep > MovementSettings.WalkAnimationIdleResetSeconds) WalkFrame = 0;
    }

    /// <summary>
    /// Faces the given direction, then moves by one tile if the destination is
    /// within bounds and walkable. Bumping into something still turns the
    /// player to face it, without animating a step.
    /// </summary>
    /// <param name="columnDelta">-1, 0, or 1.</param>
    /// <param name="rowDelta">-1, 0, or 1.</param>
    public void Move(int columnDelta, int rowDelta)
    {
        Facing = DirectionFor(columnDelta, rowDelta);

        var newColumn = Column + columnDelta;
        var newRow = Row + rowDelta;

        if (newColumn < 0 || newColumn >= _map.Columns) return;
        if (newRow < 0 || newRow >= _map.Rows) return;
        if (!_map.IsWalkable(newColumn, newRow)) return;

        Column = newColumn;
        Row = newRow;
        WalkFrame = 1 - WalkFrame;
        _timeSinceLastStep = 0f;
    }

    /// <summary>Relocates to a different map — e.g. walking through a door into another scene.</summary>
    public void WarpTo(TileMap map, int column, int row)
    {
        _map = map;
        Column = column;
        Row = row;
    }

    private static Direction DirectionFor(int columnDelta, int rowDelta) => (columnDelta, rowDelta) switch
    {
        (1, 0) => Direction.Right,
        (-1, 0) => Direction.Left,
        (0, 1) => Direction.Down,
        (0, -1) => Direction.Up,
        _ => Direction.Down,
    };
}
