using Raylib_cs;

namespace Dqh.Game.Rendering;

/// <summary>Named colors still needed once tiles/actors are drawn from real textures.</summary>
internal static class Palette
{
    public static readonly Color WindowBackground = Color.Black;

    public static readonly Color DialogueBoxBackground = new((byte)10, (byte)10, (byte)40, (byte)230);
    public static readonly Color DialogueBoxBorder = Color.White;
    public static readonly Color DialogueLineText = Color.White;
    public static readonly Color DialogueContinueHintText = Color.LightGray;

    public static readonly Color WelcomeOverlayBackground = new((byte)5, (byte)5, (byte)20, (byte)255);
}
