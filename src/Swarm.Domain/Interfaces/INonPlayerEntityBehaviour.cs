using Swarm.Domain.Entities.NonPlayerEntities;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Interfaces;

public interface INonPlayerEntityBehaviour
{
    (Direction direction, float speed)? DecideMovement(NonPlayerEntityContext<INonPlayerEntity> context);

    bool DecideAction(NonPlayerEntityContext<INonPlayerEntity> context);
}
