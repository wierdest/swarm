using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities.Behaviours;

public sealed class ChaseBehaviour(
    float speed,
    ITargetStrategy targetStrategy,
    IActionStrategy? actionStrategy,
    IDodgeStrategy dodgeStrategy, 
    IRunawayStrategy? runawayStrategy
) : NonPlayerEntityBehaviourBase(speed, actionStrategy, dodgeStrategy, runawayStrategy)
{
    protected override float RunawayMultiplier() => 2.0f;

    protected override (Direction direction, float speed)? DecidePrimaryMovement(
        NonPlayerEntityContext<INonPlayerEntity> context)
    {
        var targetPos = targetStrategy.GetTarget(context);
        if (targetPos.IsZero())
            return null;

        var dir = Direction.From(targetPos.X, targetPos.Y);
        return (dir, Speed);
    }
}
