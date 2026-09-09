namespace Dqh.Game.Settings;

/// <summary>Overworld grid dimensions, shared by rendering and movement code.</summary>
internal static class GridSettings
{
    public const int Columns = 20;
    public const int Rows = 15;
    public const int TileSizePixels = 32;

    public const int WindowWidth = Columns * TileSizePixels;
    public const int WindowHeight = Rows * TileSizePixels;
    public const int TargetFps = 60;
}
