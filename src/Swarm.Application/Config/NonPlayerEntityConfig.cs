namespace Swarm.Application.Config;

public sealed record class NonPlayerEntityConfig(
    int HP,
    float Radius,   
    float Speed,
    float? ShootRange,
    float Cooldown,
    int? Damage,
    DodgeConfig? DodgeConfig,
    RunawayConfig? RunawayConfig,
    TargetConfig? TargetConfig,
    WeaponConfig? Weapon,
    IReadOnlyList<SpawnerConfig>? Spawners


);
