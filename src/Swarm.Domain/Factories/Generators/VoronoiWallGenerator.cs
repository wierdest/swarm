using Swarm.Domain.GameObjects;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Factories.Generators;

public static class VoronoiWallGenerator
{
    public static IEnumerable<Wall> Generate(
        Vector2 start,
        Vector2 targetPos,
        Bounds levelBounds,
        int seedCount,
        float cellSize,
        float wallRadius,
        float wallDensity,
        int? seed = null)
    {
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();

        // Step 1: generate Voronoi seed points
        var seeds = new List<Vector2>(seedCount);
        for (int i = 0; i < seedCount; i++)
        {
            var x = (float)rng.NextDouble() * levelBounds.Width + levelBounds.Left;
            var y = (float)rng.NextDouble() * levelBounds.Height + levelBounds.Top;
            seeds.Add(new Vector2(x, y));
        }

        // Step 2: sample grid and mark edges between different cells
        for (float y = levelBounds.Top; y < levelBounds.Bottom; y += cellSize)
        {
            for (float x = levelBounds.Left; x < levelBounds.Right; x += cellSize)
            {
                var current = new Vector2(x, y);
                var nearest = FindNearestSeed(current, seeds);

                var right = new Vector2(x + cellSize, y);
                var down = new Vector2(x, y + cellSize);

                var nearestRight = FindNearestSeed(right, seeds);
                var nearestDown = FindNearestSeed(down, seeds);

                if (nearestRight != nearest && rng.NextDouble() < wallDensity)
                {
                    var pos = current + new Vector2(cellSize / 2f, 0);
                    if (!IsNearArea(pos, start, targetPos))
                        yield return new Wall(pos, new Radius(wallRadius), null);
                }

                if (nearestDown != nearest && rng.NextDouble() < wallDensity)
                {
                    var pos = current + new Vector2(0, cellSize / 2f);
                    if (!IsNearArea(pos, start, targetPos))
                        yield return new Wall(pos, new Radius(wallRadius), null);
                }
            }
        }
    }

    private static Vector2 FindNearestSeed(Vector2 p, IReadOnlyList<Vector2> seeds)
    {
        Vector2 nearest = seeds[0];
        float bestDist = Vector2.DistanceSquared(p, nearest);

        for (int i = 1; i < seeds.Count; i++)
        {
            float d = Vector2.DistanceSquared(p, seeds[i]);
            if (d < bestDist)
            {
                bestDist = d;
                nearest = seeds[i];
            }
        }

        return nearest;
    }

    private static bool IsNearArea(Vector2 pos, Vector2? playerPos, Vector2? targetPos)
    {
        const float minDistanceSq = 30000f; // ~170 units
        if (playerPos is Vector2 p && Vector2.DistanceSquared(pos, p) < minDistanceSq)
            return true;

        if (targetPos is Vector2 t && Vector2.DistanceSquared(pos, t) < minDistanceSq)
            return true;

        return false;
    }
}
