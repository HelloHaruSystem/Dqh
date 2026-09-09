namespace Dqh.Domain.Magic;

/// <summary>Shared validation for spell constructors, without an inheritance relationship between spell types.</summary>
internal static class SpellGuard
{
    public static void EnsureValid(string name, int manaCost)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Spell must have a name.", nameof(name));
        if (manaCost < 0)
            throw new ArgumentOutOfRangeException(nameof(manaCost), "Mana cost cannot be negative.");
    }
}
