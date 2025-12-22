using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Events;

public sealed record RadicalSpawnEvent(IReadOnlyList<Vector2> SpawnPositions) : IDomainEvent;