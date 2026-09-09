namespace Dqh.Game.World;

/// <summary>Something on the map the player can face and interact with — an NPC, a sign, and so on.</summary>
internal interface IInteractable
{
    int Column { get; }
    int Row { get; }

    /// <returns>The lines of text to show for this interaction.</returns>
    IReadOnlyList<string> Interact();
}
