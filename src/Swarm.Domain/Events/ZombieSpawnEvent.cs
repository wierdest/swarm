using Swarm.Domain.Combat;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Events;

public sealed record ZombieSpawnEvent(
    IReadOnlyList<Vector2> SpawnPositions,
    Radius Radius,
    HitPoints HitPoints,
    float Speed,
    float? TargetThreshold,
    float? DodgeThreshold,
    float? DodgeMultiplier
    ) : IDomainEvent;
