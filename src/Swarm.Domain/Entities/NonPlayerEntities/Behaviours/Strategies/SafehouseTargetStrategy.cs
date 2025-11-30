using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities.Behaviours.Strategies;

public sealed class SafehouseTargetStrategy(Vector2 safehouse) : ITargetStrategy
{
    public Vector2 GetTarget(NonPlayerEntityContext<INonPlayerEntity> context)
        => safehouse - context.Position;
    
}
