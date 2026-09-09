namespace Dqh.Game.World;

/// <summary>Something on the map the player can face and interact with — an NPC, a sign, and so on.</summary>
internal interface IInteractable
{
    int Column { get; }
    int Row { get; }

    /// <summary>Queues whatever this interaction shows — lines, a choice, or both — onto <paramref name="world"/>.</summary>
    void Interact(GameWorld world, PlayerMarker player);
}
