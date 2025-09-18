using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities.Enemies.Behaviours;

public class PatrolBehaviour(
    IReadOnlyList<Vector2> waypoints,
    float speed,
    float shootRange,
    Cooldown shootCooldown
) : IEnemyBehaviour
{
    private int _currentIndex;
    private Cooldown _cooldown = shootCooldown;
    public bool DecideAction(
        Vector2 enemyPosition,
        Vector2 playerPosition,
        DeltaTime dt)
    {
        _cooldown = _cooldown.Tick(dt);

        _cooldown = _cooldown.ConsumeIfReady(out var consumed);

        if (!consumed) return false;

        var delta = playerPosition - enemyPosition;
        if (delta.LengthSquared() <= shootRange * shootRange)
        {
            return true;
        }

        return false;
    }

     public (Direction direction, float speed)? DecideMovement(
        Vector2 enemyPosition,
        Vector2 playerPosition,
        DeltaTime dt)
    {
        if (waypoints.Count == 0)
            return null;

        var target = waypoints[_currentIndex];
        var toTarget = target - enemyPosition;

        if (toTarget.LengthSquared() < 4f)
        {
            _currentIndex = (_currentIndex + 1) % waypoints.Count;
            return null;
        }

        var dir = Direction.From(toTarget.X, toTarget.Y);
        return (dir, speed);
    }
}
