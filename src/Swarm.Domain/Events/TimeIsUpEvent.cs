using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Events;

public sealed record TimeIsUpEvent(EntityId SessionId) : IDomainEvent;
