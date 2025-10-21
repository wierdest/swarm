using Swarm.Domain.GameObjects;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Interfaces;
public interface ISpawnerPlacementStrategy
{
    IEnumerable<Vector2> GetSpawnerPositions(Wall wall, Bounds levelBounds);
}
