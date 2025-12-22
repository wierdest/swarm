using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities.Missions;

public sealed record class LevelSettings(
    Vector2 PlayerStartPosition,
    RoundTimer RoundLength,
    int WallFactorySeedCount,
    float WallDensity,
    int MaxSpawnerCount,
    float SpawnerCooldownSeconds,
    int SpawnerBatchSize
);
