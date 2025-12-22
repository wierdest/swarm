using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Events;

public sealed record HealthyInfectedEvent(EntityId Id, Vector2 Position) : IDomainEvent;

