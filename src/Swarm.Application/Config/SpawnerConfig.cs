namespace Swarm.Application.Config;

public sealed record class SpawnerConfig(
    float CooldownSeconds,
    int BatchSize,
    int? Number
);  
