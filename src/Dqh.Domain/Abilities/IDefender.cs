namespace Dqh.Domain.Abilities;

/// <summary>Can brace to reduce the next hit taken.</summary>
public interface IDefender
{
    /// <returns>Flavor text describing the guard action.</returns>
    string Guard();
}
