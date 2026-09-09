namespace Dqh.Domain.Utilities;

/// <summary>Generic search helper, reused across the party roster and the encounter log.</summary>
public static class DomainToolbox
{
    /// <returns>The first matching item, or <c>default</c> if none match.</returns>
    public static T? FindFirst<T>(IEnumerable<T> items, Func<T, bool> predicate)
    {
        foreach (var item in items)
        {
            if (predicate(item)) return item;
        }

        return default;
    }
}
