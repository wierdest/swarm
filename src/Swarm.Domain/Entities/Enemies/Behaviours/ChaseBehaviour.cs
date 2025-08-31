using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities.Enemies.Behaviours;

public sealed class ChaseBehaviour(float speed) : IEnemyBehaviour
{
    public bool DecideAction(Vector2 enemyPosition, Vector2 playerPosition, DeltaTime dt)
    {
        throw new NotImplementedException();
    }

    public (Direction direction, float speed)? DecideMovement(Vector2 enemyPosition, Vector2 playerPosition, DeltaTime dt)
    {
        var toPlayer = playerPosition - enemyPosition;

        if (toPlayer.IsZero())
            return null;

        var direction = Direction.From(toPlayer.X, toPlayer.Y);
        return (direction, speed);
    }
}
