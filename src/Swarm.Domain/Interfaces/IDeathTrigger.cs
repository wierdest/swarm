using Swarm.Domain.Primitives;

namespace Swarm.Domain.Interfaces;

public interface IDeathTrigger
{
    IEnumerable<IDomainEvent> OnDeath(Vector2 position);
}
