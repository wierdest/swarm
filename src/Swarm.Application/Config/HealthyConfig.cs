namespace Swarm.Application.Config;

public record class HealthyConfig(
    NonPlayerEntityConfig NonPlayerEntityConfig,
    float? InfectedSpeed,
    float? InfectedTargetThreshold,
    float? InfectedDodgeThreshold
);

