using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities.NonPlayerEntities.Behaviours;

public sealed class PatrolBehaviour(
    IReadOnlyList<Vector2> waypoints,
    float speed,
    IActionStrategy actionStrategy,
    Cooldown actionCooldown,
    IDodgeStrategy dodgeStrategy,
    IRunawayStrategy? runawayStrategy
) : NonPlayerEntityBehaviourBase(speed, actionStrategy, dodgeStrategy, runawayStrategy, actionCooldown)
{
    private int _currentIndex;

    protected override (Direction direction, float speed)? DecidePrimaryMovement(
        NonPlayerEntityContext<INonPlayerEntity> context)
    {
        if (waypoints.Count == 0) return null;

        var target = waypoints[_currentIndex];
        var toTarget = target - context.Position;

        if (toTarget.LengthSquared() < 4f)
        {
            _currentIndex = (_currentIndex + 1) % waypoints.Count;
            return null;
        }

        var dir = Direction.From(toTarget.X, toTarget.Y);
        return (dir, Speed);
    }
}
