namespace Swarm.Application.Config;

public record class RunawayConfig(
    int? Threshold,
    float? SafehouseWeight,
    float? AvoidPlayerWeight
);