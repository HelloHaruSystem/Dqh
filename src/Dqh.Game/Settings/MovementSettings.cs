namespace Dqh.Game.Settings;

/// <summary>
/// Timing for the player's tile-by-tile movement. One shared value drives both
/// how often a held direction repeats a step (<see cref="Dqh.Game.Input.RaylibInputSource"/>)
/// and when the walk-cycle animation settles back to standing
/// (<see cref="Dqh.Game.World.PlayerMarker"/>) — kept as a single source so the
/// two can't drift out of sync with each other.
/// </summary>
internal static class MovementSettings
{
    /// <summary>Seconds between tile steps while a direction is held.</summary>
    public const float StepIntervalSeconds = 0.25f;

    /// <summary>
    /// How long since the last step before the walk cycle resets to a standing
    /// pose. Kept well above <see cref="StepIntervalSeconds"/> so ordinary frame-time
    /// jitter between consecutive steps can't cause a spurious reset mid-walk.
    /// </summary>
    public const float WalkAnimationIdleResetSeconds = StepIntervalSeconds * 1.5f;
}
