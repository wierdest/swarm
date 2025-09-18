namespace Swarm.Application.Config;

public sealed record class WeaponConfig(
    string Name,
    int Damage,
    int MaxAmmo,
    float ProjectileSpeed,
    float ProjectileRadius,
    float RatePerSecond,
    float ProjectileLifetimeSeconds
);
