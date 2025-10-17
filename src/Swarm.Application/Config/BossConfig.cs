namespace Swarm.Application.Config;

public sealed record class BossConfig(
    IReadOnlyList<PointConfig> Waypoints,
    float Speed,
    float ShootRange,
    float Cooldown,
    int Damage,
    float ProjectileSpeed,
    float ProjectileRadius,
    float ProjectileRatePerSecond,
    float ProjectileLifetimeSeconds
);
