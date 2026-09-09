namespace Dqh.Game.World;

/// <summary>Position and identity data for a map's player spawn, NPCs, portals, and decorations.</summary>
internal sealed class MapEntities
{
    public GridPosition PlayerSpawn { get; set; } = new();
    public List<NpcData> Npcs { get; set; } = [];
    public List<PortalData> Portals { get; set; } = [];
    public List<DecorationData> Decorations { get; set; } = [];
}

internal sealed class GridPosition
{
    public int Column { get; set; }
    public int Row { get; set; }
}

/// <summary><see cref="Id"/> resolves to actual dialogue/behavior in code, not here.</summary>
internal sealed class NpcData
{
    public string Id { get; set; } = "";
    public int Column { get; set; }
    public int Row { get; set; }
}

internal sealed class PortalData
{
    public int Column { get; set; }
    public int Row { get; set; }
    public string TargetMap { get; set; } = "";
    public int TargetColumn { get; set; }
    public int TargetRow { get; set; }
}

internal sealed class DecorationData
{
    public string Id { get; set; } = "";
    public int Column { get; set; }
    public int Row { get; set; }
}
