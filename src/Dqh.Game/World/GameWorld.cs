using Dqh.Domain.Battles;
using Dqh.Domain.Encounters;
using Dqh.Domain.Party;
using Dqh.Game.Settings;

namespace Dqh.Game.World;

/// <summary>
/// The map currently loaded and active, plus the session-wide state that
/// overlays it: the current map transition, the current conversation, and
/// the party's ongoing battle, if any.
/// </summary>
internal sealed class GameWorld
{
    private readonly IEncounterGenerator _encounterGenerator;
    private readonly IBattleResolver _battleResolver;
    private (TileMap Map, MapEntities Entities) _current;

    public TileMap Map => _current.Map;
    public IReadOnlyList<DecorationData> Decorations => _current.Entities.Decorations;
    public IReadOnlyList<NpcData> Npcs => _current.Entities.Npcs;
    public GridPosition PlayerSpawn => _current.Entities.PlayerSpawn;

    public Party Party { get; }
    public MapTransition Transition { get; } = new();
    public Conversation Conversation { get; } = new();
    public Battle? ActiveBattle { get; private set; }

    /// <summary>True until the player dismisses the opening title screen.</summary>
    public bool IsShowingWelcome { get; private set; } = true;

    public GameWorld(string startingMapName, Party party, IEncounterGenerator encounterGenerator, IBattleResolver battleResolver)
    {
        _current = MapLoader.Load(startingMapName);
        Party = party;
        _encounterGenerator = encounterGenerator;
        _battleResolver = battleResolver;
    }

    public void DismissWelcome() => IsShowingWelcome = false;

    /// <summary>If the player is standing on a portal, starts the fade-out that will lead into it.</summary>
    public void CheckPortal(PlayerMarker player)
    {
        if (Transition.IsActive) return;

        var portal = _current.Entities.Portals.FirstOrDefault(p => p.Column == player.Column && p.Row == player.Row);
        if (portal is null) return;

        Transition.Start(portal.TargetMap, portal.TargetColumn, portal.TargetRow);
    }

    /// <summary>Starts the same fade transition as a portal, but repositions the player on the current map instead of loading a new one.</summary>
    public void StartRelocation(int targetColumn, int targetRow) => Transition.Start(targetMap: null, targetColumn, targetRow);

    /// <summary>Rolls for a random encounter if the player is standing on an encounter zone.</summary>
    public void CheckEncounter(PlayerMarker player)
    {
        if (Transition.IsActive || ActiveBattle is not null) return;
        if (Map.GetTile(player.Column, player.Row) != TileType.EncounterZone) return;
        if (Random.Shared.Next(100) >= EncounterSettings.TriggerChancePercent) return;

        var encounter = _encounterGenerator.Generate();
        Transition.Start(targetMap: null, player.Column, player.Row, onMidpoint: () => ActiveBattle = new Battle(encounter, Party, _battleResolver));
    }

    /// <summary>Fades back to the overworld once the current battle has finished — a party wipe is a forgiving game-over, not a dead end.</summary>
    public void EndBattle(PlayerMarker player)
    {
        if (ActiveBattle?.Result == Battle.Outcome.Lost)
        {
            foreach (var member in Party.Members) member.FullyRestore();
            Transition.Start(targetMap: null, PlayerSpawn.Column, PlayerSpawn.Row, onMidpoint: () => ActiveBattle = null);
        }
        else
        {
            Transition.Start(targetMap: null, player.Column, player.Row, onMidpoint: () => ActiveBattle = null);
        }
    }

    /// <summary>Advances an in-progress transition, relocating the player (and swapping the map, if crossing one) at the midpoint.</summary>
    public void Tick(float deltaSeconds, PlayerMarker player)
    {
        if (Transition.Tick(deltaSeconds) is not { } relocation) return;

        if (relocation.TargetMap is { } targetMap)
        {
            _current = MapLoader.Load(targetMap);
        }

        player.WarpTo(Map, relocation.TargetColumn, relocation.TargetRow);
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
        if (Transition.IsActive || Conversation.IsTalking) return;

        var (column, row) = FacedTile(player);
        var interactable = FindInteractableAt(column, row);
        interactable?.Interact(this, player);
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
