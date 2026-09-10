namespace Dqh.Game.Settings;

/// <summary>How readily standing on an encounter zone starts a fight.</summary>
internal static class EncounterSettings
{
    /// <summary>Chance, out of 100, that stepping onto an <see cref="World.TileType.EncounterZone"/> tile starts a battle.</summary>
    public const int TriggerChancePercent = 35;
}
