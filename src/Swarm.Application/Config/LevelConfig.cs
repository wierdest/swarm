namespace Swarm.Application.Config;

public sealed record class LevelConfig(
    WeaponConfig? Weapon,
    int? PlayerInitialAmmoModifier,
    AreaConfig? PlayerAreaConfig,
    AreaConfig? TargetAreaConfig,
    IReadOnlyList<AreaConfig>? Walls,
    WallGeneratorConfig? WallGeneratorConfig,
    ShooterConfig? ShooterConfig,
    NonPlayerEntityConfig? ZombieConfig,
    HealthyConfig? HealthyConfig,
    GoalConfig? GoalConfig,
    List<BombConfig>? Bombs
);
