using Dqh.Game.World;

namespace Dqh.Game.Rendering;

/// <summary>Draws a single actor at a grid cell, facing a direction. Swappable for a sprite-based renderer later.</summary>
internal interface IActorRenderer
{
    void Draw(int column, int row, Direction facing, int walkFrame, Camera camera, Viewport viewport);
}
