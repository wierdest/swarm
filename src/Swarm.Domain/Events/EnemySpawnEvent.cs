using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Events;

public sealed record EnemySpawnEvent(IReadOnlyList<Vector2> SpawnPositions) : IDomainEvent;