using Swarm.Domain.Entities.NonPlayerEntities;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Interfaces;

public interface ITargetStrategy
{
    Vector2 GetTarget(NonPlayerEntityContext<INonPlayerEntity> context);
}
