namespace Swarm.Application.Config;

public sealed record class LevelConfig(
    WeaponConfig? Weapon,
    int? PlayerInitialAmmoModifier,
    AreaConfig? PlayerAreaConfig,
    AreaConfig? TargetAreaConfig,
    IReadOnlyList<AreaConfig>? Walls,
    WallGeneratorConfig? WallGeneratorConfig,
    IReadOnlyList<SpawnerConfig>? Spawners,
    NonPlayerEntityConfig? ShooterConfig,
    NonPlayerEntityConfig? ZombieConfig,
    NonPlayerEntityConfig? HealthyConfig,
    GoalConfig? GoalConfig
);
