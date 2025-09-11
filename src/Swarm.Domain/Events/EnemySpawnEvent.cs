using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Events;

public sealed record EnemySpawnEvent(Vector2 Position, int Count) : IDomainEvent;