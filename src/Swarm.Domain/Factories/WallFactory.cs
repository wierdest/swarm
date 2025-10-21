using Swarm.Domain.Factories.Algorithms;
using Swarm.Domain.GameObjects;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Factories;

public static class WallFactory
{
    public static IEnumerable<Wall> CreateWalls(IEnumerable<(Vector2 pos, Radius radius)> definitions)
    {
        foreach (var (pos, radius) in definitions)
            yield return new Wall(pos, radius, null);
    }

    public static IEnumerable<Wall> CreateVoronoiWalls(
        Vector2 start,
        Vector2 end,
        Bounds levelBounds,
        float wallRadius = 20f,
        int seedCount = 3, // lower values + open space
        float wallDensity = 0.3f,
        int? seed = null // TODO we need to store this value to be able to replicate them, store nice layouts
    )
    {
        var walls = VoronoiWallGenerator.Generate(
            start: start,
            targetPos: end,
            levelBounds: levelBounds,
            seedCount: seedCount,
            wallRadius: wallRadius,
            wallDensity: wallDensity,
            seed: seed
        );

        foreach (var wall in walls)
        {
            yield return wall;
        }
    }

}