namespace Dqh.Game.World;

/// <summary>Which tile types block movement.</summary>
internal static class TileTraits
{
    public static bool IsWalkable(TileType tile) => tile switch
    {
        TileType.Mountain or TileType.River or TileType.Roof or TileType.Wall
            or TileType.BarCounter or TileType.BedHead or TileType.BedFoot or TileType.Table => false,
        _ => true,
    };
}
