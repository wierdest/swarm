using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities.Behaviours.Strategies;

public sealed class NearestHealthyOrPlayerTargetStrategy(
    float threshold
) : ITargetStrategy
{
    public Vector2 GetTarget(NonPlayerEntityContext<INonPlayerEntity> context)
    {
        var nearest = context.NearestHealthy;

        if (nearest is not null)
        {
            var distance = Vector2.Distance(context.Position, nearest.Position);

            if (distance > threshold)
                return nearest.Position - context.Position;
        }

        return context.PlayerPosition - context.Position;
    }
}
