using Swarm.Domain.Factories.Generators;
using Swarm.Domain.GameObjects;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Factories;

public static class WallFactory
{
    public static IEnumerable<Wall> CreateVoronoiWalls(
        Vector2 start,
        Vector2 end,
        Bounds levelBounds,
        float wallRadius,
        int seedCount, // lower values + open space
        float wallDensity,
        float cellSize,
        int minWallCount,
        int? seed = null // TODO we need to store this value to be able to replicate them, store nice layouts
    )
    {
        var walls = VoronoiWallGenerator.Generate(
            start: start,
            targetPos: end,
            levelBounds: levelBounds,
            seedCount: seedCount,
            cellSize: cellSize,
            wallRadius: wallRadius,
            wallDensity: wallDensity,
            minWallCount: minWallCount,
            seed: seed
        );

        foreach (var wall in walls)
        {
            yield return wall;
        }
    }

}
