using Dqh.Game.World;

namespace Dqh.Game.Presentation;

/// <summary>Presents one frame of the world. Swappable between a graphical window and a headless console dump.</summary>
internal interface IWorldPresenter
{
    void Present(TileMap map, IReadOnlyList<DecorationData> decorations, PlayerMarker player, float deltaSeconds);
}
