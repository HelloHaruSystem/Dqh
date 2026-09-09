using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Combatants.Monsters;
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
    private readonly MonsterRenderer _monsterRenderer;

    public RaylibWorldPresenter(
        ITileRenderer tileRenderer,
        IActorRenderer playerRenderer,
        PropRenderer propRenderer,
        NpcRenderer npcRenderer,
        MonsterRenderer monsterRenderer)
    {
        _tileRenderer = tileRenderer;
        _playerRenderer = playerRenderer;
        _propRenderer = propRenderer;
        _npcRenderer = npcRenderer;
        _monsterRenderer = monsterRenderer;
    }

    public void Present(GameWorld world, PlayerMarker player, float deltaSeconds)
    {
        Raylib.BeginDrawing();

        if (world.ActiveBattle is { } battle)
        {
            DrawBattleScreen(battle);
        }
        else
        {
            var camera = Camera.Follow(world.Map, player.Column, player.Row, GridSettings.CameraColumns, GridSettings.CameraRows);
            var viewport = Viewport.Fit(camera.VisibleColumns, camera.VisibleRows, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

            Raylib.ClearBackground(Palette.WindowBackground);
            _tileRenderer.Draw(world.Map, camera, viewport);
            _propRenderer.Draw(world.Decorations, camera, viewport);
            _npcRenderer.Draw(world.Npcs, camera, viewport);
            _playerRenderer.Draw(player.Column, player.Row, player.Facing, player.WalkFrame, camera, viewport);
        }

        if (world.Transition.Fade > 0f)
        {
            var alpha = (byte)Math.Clamp(world.Transition.Fade * 255f, 0f, 255f);
            Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), new Color((byte)0, (byte)0, (byte)0, alpha));
        }

        if (world.Conversation.ActiveChoice is { } choice)
        {
            DrawChoiceBox(choice, world.Conversation.ChoiceYesSelected);
        }
        else if (world.Conversation.ActiveDialogueLine is { } line)
        {
            DrawDialogueBox(line);
        }
        else if (!world.IsShowingWelcome && world.Transition.Fade == 0f && world.ActiveBattle is null && world.IsFacingInteractable(player))
        {
            DrawInteractPrompt();
        }

        if (world.IsShowingWelcome)
        {
            DrawWelcomeScreen();
        }

        Raylib.EndDrawing();
    }

    private void DrawBattleScreen(Battle battle)
    {
        var screenWidth = Raylib.GetScreenWidth();
        var screenHeight = Raylib.GetScreenHeight();

        Raylib.ClearBackground(Palette.WindowBackground);
        DrawMonsterRow(battle.ActiveMonsters, screenWidth, screenHeight);
        DrawPartyStatus(battle.PartyMembers, screenHeight);

        if (battle.IsShowingLog)
        {
            DrawDialogueBox(battle.LogLine!);
        }
        else if (battle.IsShowingMenu)
        {
            DrawBattleMenu(battle.MenuOptions, battle.SelectedMenuIndex, screenWidth, screenHeight);
        }
    }

    private void DrawMonsterRow(IReadOnlyList<IMonster> monsters, int screenWidth, int screenHeight)
    {
        var spriteSize = screenHeight / 4;
        var topMargin = screenHeight / 10;
        var spacing = screenWidth / (monsters.Count + 1);

        for (var i = 0; i < monsters.Count; i++)
        {
            var monster = monsters[i];
            var x = spacing * (i + 1) - spriteSize / 2;

            if (!monster.IsDefeated)
            {
                _monsterRenderer.Draw(monster.Kind, x, topMargin, spriteSize);
            }

            var hpText = monster.IsDefeated ? $"{monster.Name}: defeated" : $"{monster.Name}: {monster.CurrentHitPoints}/{monster.MaxHitPoints} HP";
            var textWidth = Raylib.MeasureText(hpText, UiSettings.ContinueHintFontSize);
            Raylib.DrawText(hpText, x + spriteSize / 2 - textWidth / 2, topMargin + spriteSize + 4, UiSettings.ContinueHintFontSize, Palette.DialogueLineText);
        }
    }

    private static void DrawPartyStatus(IReadOnlyList<Adventurer> members, int screenHeight)
    {
        var margin = UiSettings.DialogueBoxMarginPixels;
        var startY = screenHeight / 2;
        var lineHeight = UiSettings.ContinueHintFontSize + 6;

        for (var i = 0; i < members.Count; i++)
        {
            var member = members[i];
            var text = member.IsDefeated
                ? $"{member.Name}: defeated"
                : $"{member.Name}: {member.CurrentHitPoints}/{member.MaxHitPoints} HP  {member.Mana} MP";
            Raylib.DrawText(text, margin, startY + i * lineHeight, UiSettings.ContinueHintFontSize, Palette.DialogueLineText);
        }
    }

    private static void DrawBattleMenu(IReadOnlyList<string> options, int selectedIndex, int screenWidth, int screenHeight)
    {
        var margin = UiSettings.DialogueBoxMarginPixels;
        var boxHeight = screenHeight / UiSettings.DialogueBoxHeightDivisor;
        var boxY = screenHeight - boxHeight - margin;
        var lineHeight = UiSettings.DialogueLineFontSize + margin / 2;

        Raylib.DrawRectangle(margin, boxY, screenWidth - margin * 2, boxHeight, Palette.DialogueBoxBackground);
        Raylib.DrawRectangleLines(margin, boxY, screenWidth - margin * 2, boxHeight, Palette.DialogueBoxBorder);

        for (var i = 0; i < options.Count; i++)
        {
            DrawChoiceOption(options[i], margin * 2, boxY + margin + i * lineHeight, i == selectedIndex);
        }
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
