namespace Swarm.Application.Config;

public sealed record class LevelConfig(
    WeaponConfig? Weapon,
    int? PlayerInitialAmmoModifier,
    AreaConfig? PlayerAreaConfig,
    AreaConfig? TargetAreaConfig,
    IReadOnlyList<WallConfig>? Walls,
    WallGeneratorConfig? WallGeneratorConfig,
    NonPlayerEntityConfig? BossConfig,
    GoalConfig? GoalConfig
);
