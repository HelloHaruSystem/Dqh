using Dqh.Game.Settings;

namespace Dqh.Game.World;

/// <summary>
/// The map currently loaded and active. Swapped out for another map's — or the
/// player just relocated within the same one — behind a fade-to-black-and-back
/// transition.
/// </summary>
internal sealed class GameWorld
{
    private (TileMap Map, MapEntities Entities) _current;
    private PendingTransition? _pendingTransition;
    private float _transitionElapsed;
    private bool _hasSwapped;
    private readonly Queue<DialogueStep> _dialogueSteps = new();

    public TileMap Map => _current.Map;
    public IReadOnlyList<DecorationData> Decorations => _current.Entities.Decorations;
    public IReadOnlyList<NpcData> Npcs => _current.Entities.Npcs;
    public GridPosition PlayerSpawn => _current.Entities.PlayerSpawn;

    /// <summary>0 outside a transition; ramps 0→1→0 across a crossing (1 = fully faded to black).</summary>
    public float TransitionFade { get; private set; }

    public bool IsTransitioning => _pendingTransition is not null;

    /// <summary>The line currently on screen, or null when the current step isn't a plain line.</summary>
    public string? ActiveDialogueLine => _dialogueSteps.Count > 0 && _dialogueSteps.Peek() is DialogueLine line ? line.Text : null;

    /// <summary>The choice currently on screen, or null when the current step isn't a choice.</summary>
    public DialogueChoice? ActiveChoice => _dialogueSteps.Count > 0 && _dialogueSteps.Peek() is DialogueChoice choice ? choice : null;

    public bool IsTalking => _dialogueSteps.Count > 0;

    /// <summary>Which option is highlighted while <see cref="ActiveChoice"/> is showing.</summary>
    public bool ChoiceYesSelected { get; private set; } = true;

    /// <summary>True until the player dismisses the opening title screen.</summary>
    public bool IsShowingWelcome { get; private set; } = true;

    public GameWorld(string startingMapName)
    {
        _current = MapLoader.Load(startingMapName);
    }

    public void DismissWelcome() => IsShowingWelcome = false;

    /// <summary>If the player is standing on a portal, starts the fade-out that will lead into it.</summary>
    public void CheckPortal(PlayerMarker player)
    {
        if (IsTransitioning) return;

        var portal = _current.Entities.Portals.FirstOrDefault(p => p.Column == player.Column && p.Row == player.Row);
        if (portal is null) return;

        StartTransition(portal.TargetMap, portal.TargetColumn, portal.TargetRow);
    }

    /// <summary>Starts the same fade transition as a portal, but repositions the player on the current map instead of loading a new one.</summary>
    public void StartRelocation(int targetColumn, int targetRow) => StartTransition(targetMap: null, targetColumn, targetRow);

    private void StartTransition(string? targetMap, int targetColumn, int targetRow)
    {
        _pendingTransition = new PendingTransition(targetMap, targetColumn, targetRow);
        _transitionElapsed = 0f;
        _hasSwapped = false;
    }

    /// <summary>Advances an in-progress transition, relocating the player (and swapping the map, if this one has a target map) at the midpoint (full black).</summary>
    public void Tick(float deltaSeconds, PlayerMarker player)
    {
        if (_pendingTransition is not { } transition) return;

        _transitionElapsed += deltaSeconds;
        var half = TransitionSettings.FadeSeconds;

        if (!_hasSwapped && _transitionElapsed >= half)
        {
            if (transition.TargetMap is { } targetMap)
            {
                _current = MapLoader.Load(targetMap);
            }

            player.WarpTo(Map, transition.TargetColumn, transition.TargetRow);
            _hasSwapped = true;
        }

        if (_transitionElapsed >= half * 2)
        {
            TransitionFade = 0f;
            _pendingTransition = null;
            return;
        }

        TransitionFade = _transitionElapsed < half
            ? _transitionElapsed / half
            : 1f - (_transitionElapsed - half) / half;
    }

    /// <summary>Whether the tile the player is currently facing holds something interactable.</summary>
    public bool IsFacingInteractable(PlayerMarker player)
    {
        var (column, row) = FacedTile(player);
        return FindInteractableAt(column, row) is not null;
    }

    /// <summary>Interacts with whatever the player is currently facing. A no-op when there's nothing there.</summary>
    public void TryInteractWithFaced(PlayerMarker player)
    {
        if (IsTransitioning || IsTalking) return;

        var (column, row) = FacedTile(player);
        var interactable = FindInteractableAt(column, row);
        interactable?.Interact(this, player);
    }

    /// <summary>Queues plain lines to show, one at a time.</summary>
    public void QueueDialogueLines(IEnumerable<string> lines)
    {
        foreach (var text in lines)
        {
            _dialogueSteps.Enqueue(new DialogueLine(text));
        }
    }

    /// <summary>Queues a yes/no choice, run once the player picks an option.</summary>
    public void QueueChoice(string prompt, string yesLabel, string noLabel, Action onYes, Action? onNo = null) =>
        _dialogueSteps.Enqueue(new DialogueChoice(prompt, yesLabel, noLabel, onYes, onNo));

    /// <summary>Flips which option is highlighted while a choice is showing.</summary>
    public void ToggleChoiceSelection() => ChoiceYesSelected = !ChoiceYesSelected;

    /// <summary>Dismisses the current line, revealing the next step if there is one. A no-op unless a plain line is showing.</summary>
    public void AdvanceDialogue()
    {
        if (_dialogueSteps.Count > 0 && _dialogueSteps.Peek() is DialogueLine) _dialogueSteps.Dequeue();
    }

    /// <summary>Commits the highlighted option of the current choice, running its action. A no-op unless a choice is showing.</summary>
    public void CommitChoice()
    {
        if (_dialogueSteps.Count == 0 || _dialogueSteps.Peek() is not DialogueChoice choice) return;

        _dialogueSteps.Dequeue();
        var pickedYes = ChoiceYesSelected;
        ChoiceYesSelected = true;
        (pickedYes ? choice.OnYes : choice.OnNo)?.Invoke();
    }

    private IInteractable? FindInteractableAt(int column, int row) =>
        _current.Entities.Npcs.FirstOrDefault(n => n.Column == column && n.Row == row);

    /// <summary>The tile immediately in front of the player, in whichever direction they're facing.</summary>
    private static (int Column, int Row) FacedTile(PlayerMarker player) => player.Facing switch
    {
        Direction.Up => (player.Column, player.Row - 1),
        Direction.Down => (player.Column, player.Row + 1),
        Direction.Left => (player.Column - 1, player.Row),
        Direction.Right => (player.Column + 1, player.Row),
        _ => (player.Column, player.Row),
    };

    private readonly record struct PendingTransition(string? TargetMap, int TargetColumn, int TargetRow);
}
