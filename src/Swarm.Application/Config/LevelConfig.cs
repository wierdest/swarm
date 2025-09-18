namespace Swarm.Application.Config;

public sealed record class LevelConfig(
    WeaponConfig Weapon,
    AreaConfig PlayerAreaConfig,
    AreaConfig TargetAreaConfig,
    IReadOnlyList<AreaConfig> Walls,
    IReadOnlyList<SpawnerConfig> Spawners,
    BossConfig BossConfig
);
