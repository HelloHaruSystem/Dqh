namespace Dqh.Game.Settings;

/// <summary>How many tiles are visible on screen at once (the camera window), and how big those tiles start out.</summary>
internal static class GridSettings
{
    public const int CameraColumns = 13;
    public const int CameraRows = 9;
    public const int TileSizePixels = 32;
    public const int MinTileSizePixels = 8;

    public const int WindowWidth = CameraColumns * TileSizePixels;
    public const int WindowHeight = CameraRows * TileSizePixels;
    public const int MinWindowWidth = CameraColumns * MinTileSizePixels;
    public const int MinWindowHeight = CameraRows * MinTileSizePixels;
    public const int TargetFps = 60;
}
