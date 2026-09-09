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

    public TileMap Map => _current.Map;
    public IReadOnlyList<DecorationData> Decorations => _current.Entities.Decorations;
    public IReadOnlyList<NpcData> Npcs => _current.Entities.Npcs;
    public GridPosition PlayerSpawn => _current.Entities.PlayerSpawn;

    /// <summary>0 outside a transition; ramps 0→1→0 across a portal crossing (1 = fully faded to black).</summary>
    public float TransitionFade { get; private set; }

    public bool IsTransitioning => _pendingPortal is not null;

    public GameWorld(string startingMapName)
    {
        _current = MapLoader.Load(startingMapName);
    }

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
}
