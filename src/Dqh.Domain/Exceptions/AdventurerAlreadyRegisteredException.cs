namespace Dqh.Domain.Exceptions;

/// <summary>Thrown when the same adventurer is registered into a party twice.</summary>
public sealed class AdventurerAlreadyRegisteredException : Exception
{
    public AdventurerAlreadyRegisteredException()
    {
    }

    public AdventurerAlreadyRegisteredException(string message) : base(message)
    {
    }

    public AdventurerAlreadyRegisteredException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
