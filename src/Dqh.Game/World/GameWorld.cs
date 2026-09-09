namespace Dqh.Game.World;

/// <summary>The map currently loaded and active. Swapped out for another map's when the player walks onto a portal.</summary>
internal sealed class GameWorld
{
    private (TileMap Map, MapEntities Entities) _current;

    public TileMap Map => _current.Map;
    public IReadOnlyList<DecorationData> Decorations => _current.Entities.Decorations;
    public IReadOnlyList<NpcData> Npcs => _current.Entities.Npcs;
    public GridPosition PlayerSpawn => _current.Entities.PlayerSpawn;

    public GameWorld(string startingMapName)
    {
        _current = MapLoader.Load(startingMapName);
    }

    /// <summary>If the player is standing on a portal, loads its target map and warps the player there.</summary>
    public void CheckPortal(PlayerMarker player)
    {
        var portal = _current.Entities.Portals.FirstOrDefault(p => p.Column == player.Column && p.Row == player.Row);
        if (portal is null) return;

        _current = MapLoader.Load(portal.TargetMap);
        player.WarpTo(Map, portal.TargetColumn, portal.TargetRow);
    }
}
