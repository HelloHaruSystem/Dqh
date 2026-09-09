namespace Dqh.Game.Settings;

/// <summary>Layout for the on-screen text overlays: the dialogue box, the interact prompt, and the title screen.</summary>
internal static class UiSettings
{
    public const int DialogueBoxMarginPixels = 16;

    /// <summary>Dialogue box height, as a fraction of the screen height (1/this).</summary>
    public const int DialogueBoxHeightDivisor = 4;

    public const int DialogueLineFontSize = 20;
    public const int ContinueHintFontSize = 14;

    public const int InteractPromptFontSize = 18;
    public const int InteractPromptTopMarginPixels = 16;
    public const int InteractPromptPaddingPixels = 8;

    public const int TitleFontSize = 32;
}
