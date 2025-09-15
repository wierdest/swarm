using Swarm.Domain.Entities;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Events;

public sealed record EnemyFiredEvent(EntityId EnemyId, IEnumerable<Projectile> Projectiles) : IDomainEvent;