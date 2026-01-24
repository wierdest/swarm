namespace Swarm.Application.Config;

public sealed record class NonPlayerEntityConfig(
    int HP,
    float Radius,   
    float Speed,
    float Cooldown,
    int? Damage,
    DodgeConfig? DodgeConfig,
    TargetConfig? TargetConfig,
    IReadOnlyList<SpawnerConfig>? Spawners
);
