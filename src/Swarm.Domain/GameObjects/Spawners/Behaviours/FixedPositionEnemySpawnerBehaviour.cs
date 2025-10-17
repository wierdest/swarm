using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.GameObjects.Spawners.Behaviours;

public sealed class FixedPositionEnemySpawnerBehaviour(
    Vector2 position,
    float cooldownSeconds,
    Func<Vector2, INonPlayerEntity> enemyFactory
) : ISpawnerBehaviour<INonPlayerEntity>
{
    private float _timeSinceLastSpawn = 0f;
    
    public INonPlayerEntity? TrySpawn(float deltaSeconds, Bounds stage)
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
