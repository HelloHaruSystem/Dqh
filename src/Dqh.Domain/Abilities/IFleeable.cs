namespace Dqh.Domain.Abilities;

/// <summary>Can attempt to escape from battle.</summary>
public interface IFleeable
{
    /// <returns>Whether the escape attempt succeeded.</returns>
    bool AttemptFlee();
}
