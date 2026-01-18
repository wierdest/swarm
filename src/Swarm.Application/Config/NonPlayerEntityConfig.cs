namespace Swarm.Application.Config;

public sealed record class NonPlayerEntityConfig(
    float HP,
    float Radius,   
    float Speed,
    float? ShootRange,
    float Cooldown,
    int? Damage,
    float? DodgeThreshold,
    float? DodgeSpeedMultiplier,
    int? RunawayThreshold,
    float? RunawaySafehouseWeight,
    float? RunawayAvoidPlayerWeight,
    IReadOnlyList<PointConfig>? PatrolPoints,
    WeaponConfig? Weapon

);
