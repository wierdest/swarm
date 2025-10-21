using System;
using System.Collections.Generic;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Factories.Algorithms;

public static class SplinePathGenerator
{
    public static IEnumerable<Vector2> GeneratePath(Vector2 start, Vector2 end, int segmentCount = 5, float noiseAmplitude = 80f, int? seed = null)
    {
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();
        var points = new List<Vector2> { start };

        for (int i = 1; i < segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            var basePoint = Vector2.Lerp(start, end, t);
            var jitter = new Vector2(
                (float)(rng.NextDouble() * 2 - 1) * noiseAmplitude,
                (float)(rng.NextDouble() * 2 - 1) * noiseAmplitude
            );
            points.Add(basePoint + jitter);
        }

        points.Add(end);
        return points;
    }
}
