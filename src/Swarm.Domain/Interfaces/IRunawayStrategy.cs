using Swarm.Domain.Entities.NonPlayerEntities;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Interfaces;

public interface IRunawayStrategy
{
    Direction? DecideRunaway(NonPlayerEntityContext<INonPlayerEntity> context);
}
