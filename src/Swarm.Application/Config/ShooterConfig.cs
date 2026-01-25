namespace Swarm.Application.Config;

public record class ShooterConfig(
    NonPlayerEntityConfig NonPlayerEntityConfig,
    float? ShootRange,
    RunawayConfig? RunawayConfig,
    WeaponConfig? Weapon,
    int? MinionSpawnCount,
    NonPlayerEntityConfig? ZombieSpawnedOnDeathTriggerConfig
);
