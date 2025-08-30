using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Application.Contracts.Behaviours;

public sealed class FixedPositionSpawnBehaviour(
    Vector2 position,
    float cooldownSeconds,
    Func<Vector2, IEnemy> enemyFactory
) : IEnemySpawnerBehaviour
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

