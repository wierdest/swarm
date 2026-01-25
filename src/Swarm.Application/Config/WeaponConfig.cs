namespace Swarm.Application.Config;

public sealed record class WeaponConfig(
    string Name,
    int Damage,
    int MaxAmmo,
    float RatePerSecond,
    float ProjectileSpeed,
    float ProjectileRadius,
    float ProjectileLifetimeSeconds
);
