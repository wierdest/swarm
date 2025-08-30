using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Interfaces;

public interface IEnemyBehaviour
{
    (Direction direction, float speed)? DecideMovement(Vector2 enemyPosition, Vector2 playerPosition, DeltaTime dt);

    bool DecideAction(Vector2 enemyPosition, Vector2 playerPosition, DeltaTime dt);
}
