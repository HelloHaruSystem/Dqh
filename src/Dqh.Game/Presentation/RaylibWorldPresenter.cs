using Dqh.Game.Rendering;
using Dqh.Game.Settings;
using Dqh.Game.World;
using Raylib_cs;

namespace Dqh.Game.Presentation;

/// <summary>Draws one frame to the Raylib window, delegating actual shape drawing to the tile/actor renderers.</summary>
internal sealed class RaylibWorldPresenter : IWorldPresenter
{
    private readonly ITileRenderer _tileRenderer;
    private readonly IActorRenderer _playerRenderer;
    private readonly PropRenderer _propRenderer;
    private readonly NpcRenderer _npcRenderer;

    public RaylibWorldPresenter(ITileRenderer tileRenderer, IActorRenderer playerRenderer, PropRenderer propRenderer, NpcRenderer npcRenderer)
    {
        _tileRenderer = tileRenderer;
        _playerRenderer = playerRenderer;
        _propRenderer = propRenderer;
        _npcRenderer = npcRenderer;
    }

    public void Present(GameWorld world, PlayerMarker player, float deltaSeconds)
    {
        var camera = Camera.Follow(world.Map, player.Column, player.Row, GridSettings.CameraColumns, GridSettings.CameraRows);
        var viewport = Viewport.Fit(camera.VisibleColumns, camera.VisibleRows, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Palette.WindowBackground);
        _tileRenderer.Draw(world.Map, camera, viewport);
        _propRenderer.Draw(world.Decorations, camera, viewport);
        _npcRenderer.Draw(world.Npcs, camera, viewport);
        _playerRenderer.Draw(player.Column, player.Row, player.Facing, player.WalkFrame, camera, viewport);

        if (world.TransitionFade > 0f)
        {
            var alpha = (byte)Math.Clamp(world.TransitionFade * 255f, 0f, 255f);
            Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), new Color((byte)0, (byte)0, (byte)0, alpha));
        }

        if (world.ActiveChoice is { } choice)
        {
            DrawChoiceBox(choice, world.ChoiceYesSelected);
        }
        else if (world.ActiveDialogueLine is { } line)
        {
            DrawDialogueBox(line);
        }
        else if (!world.IsShowingWelcome && world.TransitionFade == 0f && world.IsFacingInteractable(player))
        {
            DrawInteractPrompt();
        }

        if (world.IsShowingWelcome)
        {
            DrawWelcomeScreen();
        }

        Raylib.EndDrawing();
    }

    private static void DrawDialogueBox(string line)
    {
        var margin = UiSettings.DialogueBoxMarginPixels;
        var screenWidth = Raylib.GetScreenWidth();
        var screenHeight = Raylib.GetScreenHeight();
        var boxHeight = screenHeight / UiSettings.DialogueBoxHeightDivisor;
        var boxY = screenHeight - boxHeight - margin;

        Raylib.DrawRectangle(margin, boxY, screenWidth - margin * 2, boxHeight, Palette.DialogueBoxBackground);
        Raylib.DrawRectangleLines(margin, boxY, screenWidth - margin * 2, boxHeight, Palette.DialogueBoxBorder);
        Raylib.DrawText(line, margin * 2, boxY + margin, UiSettings.DialogueLineFontSize, Palette.DialogueLineText);
        Raylib.DrawText(
            "[Enter/Space/E] continue",
            margin * 2,
            boxY + boxHeight - margin - UiSettings.ContinueHintFontSize,
            UiSettings.ContinueHintFontSize,
            Palette.DialogueContinueHintText);
    }

    private static void DrawChoiceBox(DialogueChoice choice, bool yesSelected)
    {
        var margin = UiSettings.DialogueBoxMarginPixels;
        var screenWidth = Raylib.GetScreenWidth();
        var screenHeight = Raylib.GetScreenHeight();
        var boxHeight = screenHeight / UiSettings.DialogueBoxHeightDivisor;
        var boxY = screenHeight - boxHeight - margin;
        var lineHeight = UiSettings.DialogueLineFontSize + margin / 2;

        Raylib.DrawRectangle(margin, boxY, screenWidth - margin * 2, boxHeight, Palette.DialogueBoxBackground);
        Raylib.DrawRectangleLines(margin, boxY, screenWidth - margin * 2, boxHeight, Palette.DialogueBoxBorder);
        Raylib.DrawText(choice.Prompt, margin * 2, boxY + margin, UiSettings.DialogueLineFontSize, Palette.DialogueLineText);

        var optionsY = boxY + margin + lineHeight;
        DrawChoiceOption(choice.YesLabel, margin * 2, optionsY, yesSelected);
        DrawChoiceOption(choice.NoLabel, margin * 2, optionsY + lineHeight, !yesSelected);

        Raylib.DrawText(
            "[Up/Down] select   [Enter/Space/E] confirm",
            margin * 2,
            boxY + boxHeight - margin - UiSettings.ContinueHintFontSize,
            UiSettings.ContinueHintFontSize,
            Palette.DialogueContinueHintText);
    }

    private static void DrawChoiceOption(string label, int x, int y, bool selected)
    {
        var text = (selected ? "> " : "  ") + label;
        Raylib.DrawText(text, x, y, UiSettings.DialogueLineFontSize, selected ? Palette.DialogueLineText : Palette.DialogueContinueHintText);
    }

    private static void DrawInteractPrompt()
    {
        const string text = "Press Enter to chat";
        var fontSize = UiSettings.InteractPromptFontSize;
        var padding = UiSettings.InteractPromptPaddingPixels;
        var textWidth = Raylib.MeasureText(text, fontSize);
        var screenWidth = Raylib.GetScreenWidth();

        var boxWidth = textWidth + padding * 2;
        var boxX = (screenWidth - boxWidth) / 2;
        var boxY = UiSettings.InteractPromptTopMarginPixels;
        var boxHeight = fontSize + padding * 2;

        Raylib.DrawRectangle(boxX, boxY, boxWidth, boxHeight, Palette.DialogueBoxBackground);
        Raylib.DrawRectangleLines(boxX, boxY, boxWidth, boxHeight, Palette.DialogueBoxBorder);
        Raylib.DrawText(text, boxX + padding, boxY + padding, fontSize, Palette.DialogueLineText);
    }

    private static void DrawWelcomeScreen()
    {
        const string title = "Welcome to DQH";
        const string hint = "Press Enter to begin";

        var screenWidth = Raylib.GetScreenWidth();
        var screenHeight = Raylib.GetScreenHeight();

        Raylib.DrawRectangle(0, 0, screenWidth, screenHeight, Palette.WelcomeOverlayBackground);

        var titleWidth = Raylib.MeasureText(title, UiSettings.TitleFontSize);
        var hintWidth = Raylib.MeasureText(hint, UiSettings.ContinueHintFontSize);

        Raylib.DrawText(
            title,
            (screenWidth - titleWidth) / 2,
            screenHeight / 2 - UiSettings.TitleFontSize,
            UiSettings.TitleFontSize,
            Palette.DialogueLineText);
        Raylib.DrawText(
            hint,
            (screenWidth - hintWidth) / 2,
            screenHeight / 2 + UiSettings.TitleFontSize,
            UiSettings.ContinueHintFontSize,
            Palette.DialogueContinueHintText);
    }
}
