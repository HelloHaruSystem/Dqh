namespace Dqh.Domain.Exceptions;

/// <summary>Thrown when a caster is told to cast a spell it doesn't know.</summary>
public sealed class UnknownSpellException : Exception
{
    public UnknownSpellException()
    {
    }

    public UnknownSpellException(string message) : base(message)
    {
    }

    public UnknownSpellException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
