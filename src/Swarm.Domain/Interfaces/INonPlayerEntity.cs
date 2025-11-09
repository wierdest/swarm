using Swarm.Domain.Entities.NonPlayerEntities;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Interfaces;

public interface INonPlayerEntity : ILivingEntity
{
    Direction Rotation { get; }
    void Tick(NonPlayerEntityContext<INonPlayerEntity> context);
    void RevertLastMovement();
    IReadOnlyList<IDomainEvent>? DomainEvents { get; }
    void ClearDomainEvents();
}
