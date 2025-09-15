using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Events;
public sealed record TimeUpdatedEvent(EntityId SessionId, RoundTimer Timer) : IDomainEvent;
