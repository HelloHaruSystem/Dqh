using Raylib_cs;

namespace Dqh.Game.Timing;

/// <summary>Reads the real elapsed time between frames from Raylib.</summary>
internal sealed class RaylibClock : IClock
{
    public float DeltaSeconds => Raylib.GetFrameTime();
}
