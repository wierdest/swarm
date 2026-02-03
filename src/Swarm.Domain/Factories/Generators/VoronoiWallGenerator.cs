using Swarm.Domain.GameObjects;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Factories.Generators;

public static class VoronoiWallGenerator
{
    public static IEnumerable<Wall> Generate(
        Vector2 startPos,
        Vector2 targetPos,
        Bounds levelBounds,
        int seedCount,
        float cellSize,
        float wallRadius,
        float wallDensity,
        int minWallCount,
        int? seed = null,
        float? corridorWidthMultiplier = null)
    {
        const int maxAttempts = 8;
        var requiredWallCount = Math.Max(1, minWallCount);
        int attempts = 0;
        var walls = new List<Wall>();

        while (walls.Count < requiredWallCount && attempts < maxAttempts)
        {
            var attemptSeed = seed.HasValue ? seed.Value + attempts : Environment.TickCount + attempts;
            var rng = new Random(attemptSeed);
            walls.Clear();
            GenerateOnce(
                startPos,
                targetPos,
                levelBounds,
                seedCount,
                cellSize,
                wallRadius,
                wallDensity,
                corridorWidthMultiplier,
                rng,
                walls);
            attempts++;
        }

        return walls;
    }

    private static void GenerateOnce(
        Vector2 start,
        Vector2 targetPos,
        Bounds levelBounds,
        int seedCount,
        float cellSize,
        float wallRadius,
        float wallDensity,
        float? corridorWidthMultiplier,
        Random rng,
        List<Wall> walls)
    {
        var widthMultiplier = corridorWidthMultiplier ?? 1.1f;
        var baseWidth = wallRadius >= cellSize ? wallRadius : cellSize;
        var corridorHalfWidth = baseWidth * widthMultiplier;

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
                    if (!IsNearArea(pos, start, targetPos) && !IsNearPath(pos, start, targetPos, corridorHalfWidth))
                        walls.Add(new Wall(pos, new Radius(wallRadius), null));
                }

                if (nearestDown != nearest && rng.NextDouble() < wallDensity)
                {
                    var pos = current + new Vector2(0, cellSize / 2f);
                    if (!IsNearArea(pos, start, targetPos) && !IsNearPath(pos, start, targetPos, corridorHalfWidth))
                        walls.Add(new Wall(pos, new Radius(wallRadius), null));
                }
            }
        }
    }

    private static Vector2 FindNearestSeed(Vector2 p, List<Vector2> seeds)
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

    private static bool IsNearArea(Vector2 pos, Vector2? startPos, Vector2? targetPos)
    {
        const float minDistanceSq = 30000f;
        if (startPos is Vector2 p && Vector2.DistanceSquared(pos, p) < minDistanceSq)
            return true;

        if (targetPos is Vector2 t && Vector2.DistanceSquared(pos, t) < minDistanceSq)
            return true;

        return false;
    }

    private static bool IsNearPath(Vector2 pos, Vector2 start, Vector2 target, float corridorHalfWidth)
    {
        var distSq = DistancePointToSegmentSquared(pos, start, target);
        return distSq <= corridorHalfWidth * corridorHalfWidth;
    }

    private static float DistancePointToSegmentSquared(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        var ap = p - a;
        var abLenSq = ab.LengthSquared();
        if (abLenSq <= 0.0001f)
            return Vector2.DistanceSquared(p, a);

        var t = Vector2.Dot(ap, ab) / abLenSq;
        if (t < 0f) t = 0f;
        else if (t > 1f) t = 1f;
        var closest = a + ab * t;
        return Vector2.DistanceSquared(p, closest);
    }
}
