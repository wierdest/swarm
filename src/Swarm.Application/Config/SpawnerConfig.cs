using Swarm.Domain.GameObjects.Spawners;

namespace Swarm.Application.Config;

public sealed record class SpawnerConfig(
    string BehaviourType,
    float CooldownSeconds,
    string SpawnObjectType,
    int BatchSize
);  
