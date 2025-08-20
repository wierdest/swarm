namespace Swarm.Application.Contracts;

public readonly record struct WeaponConfig(
    int Damage,
    float ProjectileSpeed,
    float ProjectileRadius,
    float RatePerSecond,
    float ProjectileLifetimeSeconds
);
