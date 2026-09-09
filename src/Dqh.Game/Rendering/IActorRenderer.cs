namespace Dqh.Game.Rendering;

/// <summary>Draws a single actor at a grid cell. Swappable for a sprite-based renderer later.</summary>
internal interface IActorRenderer
{
    void Draw(int column, int row);
}
