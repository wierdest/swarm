namespace Swarm.Application.Contracts;

public readonly record struct LevelConfig(
    WeaponConfig Weapon,
    AreaConfig PlayerAreaConfig,
    AreaConfig TargetAreaConfig,
    IReadOnlyList<AreaConfig> Walls,
    IReadOnlyList<SpawnerConfig> Spawners
);
