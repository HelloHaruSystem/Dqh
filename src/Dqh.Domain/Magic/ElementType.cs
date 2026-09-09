namespace Dqh.Domain.Magic;

/// <summary>
/// Damage type, used to match spells/attacks against monster weaknesses.
/// Mirrors the classic DQ elemental spell families: Fire (Frizz/Sizz), Ice
/// (Crack), Wind (Woosh), Explosion (Bang). <see cref="Physical"/> covers
/// weapon attacks, which are their own resistance category rather than a spell line.
/// </summary>
public enum ElementType
{
    Physical = 1,
    Fire = 2,
    Ice = 3,
    Wind = 4,
    Explosion = 5,
}
