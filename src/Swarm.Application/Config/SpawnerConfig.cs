namespace Swarm.Application.Config;

public sealed record class SpawnerConfig(
    float CooldownSeconds,
    string SpawnObjectType,
    int BatchSize
);  
