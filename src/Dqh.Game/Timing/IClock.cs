namespace Dqh.Game.Timing;

/// <summary>How much time elapsed since the last frame. Swappable so the loop doesn't need to know about Raylib.</summary>
internal interface IClock
{
    float DeltaSeconds { get; }
}
