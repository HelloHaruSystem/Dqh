using Dqh.Game.Settings;

namespace Dqh.Game.World;

/// <summary>Fade-to-black-and-back timing for a map crossing or a same-map relocation.</summary>
internal sealed class MapTransition
{
    private PendingTransition? _pending;
    private float _elapsed;
    private bool _hasCrossedMidpoint;

    /// <summary>0 outside a transition; ramps 0→1→0 across a crossing (1 = fully faded to black).</summary>
    public float Fade { get; private set; }

    public bool IsActive => _pending is not null;

    /// <param name="targetMap">Null to reposition on the current map instead of loading a new one.</param>
    /// <param name="onMidpoint">Runs once, exactly at full black, alongside the relocation itself.</param>
    public void Start(string? targetMap, int targetColumn, int targetRow, Action? onMidpoint = null)
    {
        _pending = new PendingTransition(targetMap, targetColumn, targetRow, onMidpoint);
        _elapsed = 0f;
        _hasCrossedMidpoint = false;
    }

    /// <summary>Advances the fade. Returns the relocation to apply the instant the midpoint (full black) is crossed, once.</summary>
    public (string? TargetMap, int TargetColumn, int TargetRow)? Tick(float deltaSeconds)
    {
        if (_pending is not { } pending) return null;

        _elapsed += deltaSeconds;
        var half = TransitionSettings.FadeSeconds;
        (string?, int, int)? reached = null;

        if (!_hasCrossedMidpoint && _elapsed >= half)
        {
            _hasCrossedMidpoint = true;
            reached = (pending.TargetMap, pending.TargetColumn, pending.TargetRow);
            pending.OnMidpoint?.Invoke();
        }

        if (_elapsed >= half * 2)
        {
            Fade = 0f;
            _pending = null;
        }
        else
        {
            Fade = _elapsed < half ? _elapsed / half : 1f - (_elapsed - half) / half;
        }

        return reached;
    }

    private readonly record struct PendingTransition(string? TargetMap, int TargetColumn, int TargetRow, Action? OnMidpoint);
}
