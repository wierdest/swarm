using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities.Behaviours;

public sealed class ChaseBehaviour(
    float speed,
    IActionStrategy? actionStrategy,
    IDodgeStrategy dodgeStrategy,
    IRunawayStrategy? runawayStrategy
) : NonPlayerEntityBehaviourBase(speed, actionStrategy, dodgeStrategy, runawayStrategy)
{
    protected override float RunawayMultiplier() => 2.0f;

    protected override (Direction direction, float speed)? DecidePrimaryMovement(NonPlayerEntityContext context)
    {
        var toPlayer = context.PlayerPosition - context.Position;
        if (toPlayer.IsZero())
            return null;

        var dir = Direction.From(toPlayer.X, toPlayer.Y);
        return (dir, Speed);
    }
}
