using Dqh.Game.Settings;

namespace Dqh.Game.World;

/// <summary>
/// The map currently loaded and active. Swapped out for another map's when the
/// player walks onto a portal, behind a fade-to-black-and-back transition.
/// </summary>
internal sealed class GameWorld
{
    private (TileMap Map, MapEntities Entities) _current;
    private PortalData? _pendingPortal;
    private float _transitionElapsed;
    private bool _hasSwapped;
    private readonly Queue<string> _dialogueLines = new();

    public TileMap Map => _current.Map;
    public IReadOnlyList<DecorationData> Decorations => _current.Entities.Decorations;
    public IReadOnlyList<NpcData> Npcs => _current.Entities.Npcs;
    public GridPosition PlayerSpawn => _current.Entities.PlayerSpawn;

    /// <summary>0 outside a transition; ramps 0→1→0 across a portal crossing (1 = fully faded to black).</summary>
    public float TransitionFade { get; private set; }

    public bool IsTransitioning => _pendingPortal is not null;

    /// <summary>The line currently on screen, or null when nobody is being spoken to.</summary>
    public string? ActiveDialogueLine => _dialogueLines.Count > 0 ? _dialogueLines.Peek() : null;

    public bool IsTalking => _dialogueLines.Count > 0;

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

        _pendingPortal = portal;
        _transitionElapsed = 0f;
        _hasSwapped = false;
    }

    /// <summary>Advances an in-progress transition, swapping the map and warping the player at the midpoint (full black).</summary>
    public void Tick(float deltaSeconds, PlayerMarker player)
    {
        if (_pendingPortal is not { } portal) return;

        _transitionElapsed += deltaSeconds;
        var half = TransitionSettings.FadeSeconds;

        if (!_hasSwapped && _transitionElapsed >= half)
        {
            _current = MapLoader.Load(portal.TargetMap);
            player.WarpTo(Map, portal.TargetColumn, portal.TargetRow);
            _hasSwapped = true;
        }

        if (_transitionElapsed >= half * 2)
        {
            TransitionFade = 0f;
            _pendingPortal = null;
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

    /// <summary>Interacts with whatever the player is currently facing, queuing its lines. A no-op when there's nothing there.</summary>
    public void TryInteractWithFaced(PlayerMarker player)
    {
        if (IsTransitioning || IsTalking) return;

        var (column, row) = FacedTile(player);
        var interactable = FindInteractableAt(column, row);
        if (interactable is null) return;

        foreach (var line in interactable.Interact())
        {
            _dialogueLines.Enqueue(line);
        }
    }

    /// <summary>Dismisses the current line, revealing the next one if there is one. A no-op when nobody is talking.</summary>
    public void AdvanceDialogue()
    {
        if (_dialogueLines.Count > 0) _dialogueLines.Dequeue();
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
}
