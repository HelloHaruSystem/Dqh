namespace Dqh.Game.Timing;

/// <summary>Headless mode blocks on stdin rather than running at a real frame rate, so it reports a fixed nominal tick.</summary>
internal sealed class HeadlessClock : IClock
{
    private const float NominalTickSeconds = 1f / 60f;

    public float DeltaSeconds => NominalTickSeconds;
}
