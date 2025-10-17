using Swarm.Domain.Entities.NonPlayerEntities;
using Swarm.Domain.Time;

namespace Swarm.Domain.Interfaces;

public interface IActionStrategy
{
    bool ShouldAct(NonPlayerEntityContext context, ref Cooldown cooldown);
}
