using Swarm.Domain.Entities.NonPlayerEntities;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Interfaces;

public interface IDodgeStrategy
{
    Direction? DecideDodge(NonPlayerEntityContext context);
}
