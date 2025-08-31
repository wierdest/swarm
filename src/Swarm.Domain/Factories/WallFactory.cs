using Swarm.Domain.Primitives;

namespace Swarm.Domain.Factories;

public static class WallFactory
{
    public static IEnumerable<Wall> CreateWalls(IEnumerable<(Vector2 pos, Radius radius)> definitions)
    {
        foreach (var (pos, radius) in definitions)
            yield return new Wall(pos, radius);
    }
}