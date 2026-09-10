namespace Dqh.Game.Settings;

/// <summary>
/// Timing for the player's tile-by-tile movement.
/// </summary>
internal static class MovementSettings
{
    /// <summary>Seconds between tile steps while a direction is held.</summary>
    public const float StepIntervalSeconds = 0.25f;

    /// <summary>
    /// How long since the last step before the walk cycle resets to a standing
    /// pose.
    /// </summary>
    public const float WalkAnimationIdleResetSeconds = StepIntervalSeconds * 1.5f;
}
