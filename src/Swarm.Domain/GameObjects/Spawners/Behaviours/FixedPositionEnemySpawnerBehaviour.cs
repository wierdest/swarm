using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.Spawners.Behaviours;

public sealed class FixedPositionEnemySpawnerBehaviour(
    Vector2 position,
    float cooldownSeconds,
    Func<Vector2, IEnemy> enemyFactory
) : ISpawnerBehaviour<IEnemy>
{
    private float _timeSinceLastSpawn = 0f;

    public IEnemy? TrySpawn(float deltaSeconds, Bounds stage)
    {
        _timeSinceLastSpawn += deltaSeconds;

        if (_timeSinceLastSpawn >= cooldownSeconds)
        {
            _timeSinceLastSpawn = 0f;
            return enemyFactory(position);
        }

        return null;
    }
}
