using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities.Behaviours.Strategies;

public sealed class PlayerTargetStrategy : ITargetStrategy
{
    public Vector2 GetTarget(NonPlayerEntityContext<INonPlayerEntity> context) 
        => context.PlayerPosition - context.Position;
    
}
