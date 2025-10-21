using Swarm.Domain.GameObjects;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Factories.Strategies;

public sealed class OpenSideSpawnerStrategy(IEnumerable<Wall> allWalls, float checkDistance = 80f, int maxPerWall = 4) : ISpawnerPlacementStrategy
{
    private readonly IEnumerable<Wall> _allWalls = allWalls;
    private readonly float _checkDistance = checkDistance;
    private readonly int _maxPerWall = maxPerWall;

    public IEnumerable<Vector2> GetSpawnerPositions(Wall wall, Bounds levelBounds)
    {
        var spawners = new List<Vector2>();
        var pos = wall.Position;
        var r = wall.Radius.Value;

        // Directions: Up, Down, Left, Right
        var directions = new[]
        {
            new Vector2(0, -1), // Up
            new Vector2(0, 1),  // Down
            new Vector2(-1, 0), // Left
            new Vector2(1, 0)   // Right
        };

        foreach (var dir in directions)
        {
            if (spawners.Count >= _maxPerWall)
                break;

            var checkPos = pos + dir * _checkDistance;

            // Check if another wall is too close in this direction
            bool blocked = _allWalls.Any(w =>
                w != wall &&
                Vector2.DistanceSquared(checkPos, w.Position) < Math.Pow(_checkDistance * 0.8f, 2)
            );

            if (blocked)
                continue;

            // Valid side → compute final spawner offset slightly outside wall
            var spawnerPos = pos + dir * (r + 10f);
            if (levelBounds.Contains(spawnerPos))
                spawners.Add(spawnerPos);
        }

        return spawners;
    }
}
