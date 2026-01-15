namespace Swarm.Application.Config;

public sealed record class WallConfig(
    AreaConfig Area,
    SpawnerConfig? Spawner
);
