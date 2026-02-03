using Swarm.Domain.Factories.Generators;
using Swarm.Domain.GameObjects;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Factories;

public static class WallFactory
{
    public static IEnumerable<Wall> CreateWallsFromList(
        IEnumerable<(Vector2 position, Radius radius)> wallConfigs
    )
    {
        foreach (var (position, radius) in wallConfigs)
        {
            yield return new Wall(position, radius, spawners: null);
        }
    }

    public static IEnumerable<Wall> CreateVoronoiWalls(
        Vector2 start,
        Vector2 end,
        Bounds levelBounds,
        float wallRadius,
        int seedCount, // lower values; + open space
        float wallDensity,
        float cellSize,
        int minWallCount,
        int? seed = null,
        float? corridorWidthMultiplier = null
    )
    {
        var walls = VoronoiWallGenerator.Generate(
            startPos: start,
            targetPos: end,
            levelBounds: levelBounds,
            seedCount: seedCount,
            cellSize: cellSize,
            wallRadius: wallRadius,
            wallDensity: wallDensity,
            minWallCount: minWallCount,
            seed: seed,
            corridorWidthMultiplier: corridorWidthMultiplier
        );

        foreach (var wall in walls)
        {
            yield return wall;
        }
    }

}
