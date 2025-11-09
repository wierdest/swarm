using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities.Behaviours.Strategies;

public sealed class NearestPersonOrPlayerTargetStrategy(
    float threshold
) : ITargetStrategy
{
    public Vector2 GetTarget(NonPlayerEntityContext<INonPlayerEntity> context)
    {
        var nearestPerson = context.NearestPerson;

        if (nearestPerson is not null)
        {
            var distance = Vector2.Distance(context.Position, nearestPerson.Position);

            if (distance > threshold)
                return nearestPerson.Position - context.Position;
        }

        return context.PlayerPosition - context.Position;
    }
}
