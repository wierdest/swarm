namespace Swarm.Application.Config;

public sealed record class WeaponConfig(
    int Damage,
    float ProjectileSpeed,
    float ProjectileRadius,
    float RatePerSecond,
    float ProjectileLifetimeSeconds
);
