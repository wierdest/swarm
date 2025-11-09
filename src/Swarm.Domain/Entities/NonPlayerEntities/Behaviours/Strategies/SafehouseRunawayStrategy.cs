using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities.Behaviours.Strategies;

public class SafehouseRunawayStrategy(
    int hitPointsThreshold,
    Vector2 safeHouse,
    float safeHouseWeight,
    float avoidPlayerWeight
) : IRunawayStrategy
{
    public Direction? DecideRunaway(NonPlayerEntityContext<INonPlayerEntity> context)
    {
        if (context.HP >= hitPointsThreshold) return null;

        var enemyPos = context.Position;
        var playerPos = context.PlayerPosition;

        // vector to safe house
        var toSafe = safeHouse - enemyPos;

        // vector away ffrom player
        var awayFromPlayer = enemyPos - playerPos;

        // weighted steering
        var steer = toSafe.Normalized() * safeHouseWeight + awayFromPlayer.Normalized() * avoidPlayerWeight;

        return Direction.From(steer.X, steer.Y);
    }
}
