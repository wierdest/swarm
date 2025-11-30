using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities.Behaviours.Strategies;

public class PlayerToSafehouseRunawayStrategy(
    int threshold,
    Vector2 safehouse,
    float safehouseWeight,
    float avoidPlayerWeight
) : IRunawayStrategy
{
    public Direction? DecideRunaway(NonPlayerEntityContext<INonPlayerEntity> context)
    {
        if (context.HP >= threshold) return null;

        var entityPos = context.Position;
        var playerPos = context.PlayerPosition;

        // vector to safe house
        var toSafe = safehouse - entityPos;

        // vector away ffrom player
        var awayFromPlayer = entityPos - playerPos;

        // weighted steering
        var steer = toSafe.Normalized() * safehouseWeight + awayFromPlayer.Normalized() * avoidPlayerWeight;

        return Direction.From(steer.X, steer.Y);
    }
}
