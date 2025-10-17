using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities.NonPlayerEntities.Behaviours;

public class PatrolBehaviour(
    IReadOnlyList<Vector2> waypoints,
    float speed,
    IActionStrategy actionStrategy,
    Cooldown shootCooldown, // stateful so stores it in a private 
    IDodgeStrategy dodgeStrategy, // stateless so use directly
    IRunawayStrategy? runawayStrategy
) : INonPlayerEntityBehaviour
{
    private int _currentIndex;
    private Cooldown _cooldown = shootCooldown;

    public bool DecideAction(NonPlayerEntityContext context) => actionStrategy.ShouldAct(context, ref _cooldown);

    public (Direction direction, float speed)? DecideMovement(NonPlayerEntityContext context)
    {
        var dodgeDir = dodgeStrategy.DecideDodge(context);

        if (dodgeDir is not null)
            return (dodgeDir.Value, speed);

        if (runawayStrategy is not null)
        {
            var runawayDir = runawayStrategy.DecideRunaway(context);
            if (runawayDir is not null)
                return (runawayDir.Value, speed);
        }
        
        if (waypoints.Count == 0)
            return null;

        var target = waypoints[_currentIndex];
        var toTarget = target - context.Position;

        if (toTarget.LengthSquared() < 4f)
        {
            _currentIndex = (_currentIndex + 1) % waypoints.Count;
            return null;
        }

        var dir = Direction.From(toTarget.X, toTarget.Y);
        return (dir, speed);
    }
}
