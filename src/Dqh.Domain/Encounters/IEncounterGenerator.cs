namespace Dqh.Domain.Encounters;

/// <summary>Produces a random monster encounter — the swappable seam for how encounters are generated.</summary>
public interface IEncounterGenerator
{
    Encounter Generate();
}
