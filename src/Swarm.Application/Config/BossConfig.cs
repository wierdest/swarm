namespace Swarm.Application.Config;

public sealed record class BossConfig(
    IReadOnlyList<PointConfig> Waypoints,
    float Speed,
    float ShootRange,
    float Cooldown
);
