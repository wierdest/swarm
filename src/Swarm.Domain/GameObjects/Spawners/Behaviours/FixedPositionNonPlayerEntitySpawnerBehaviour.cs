using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.GameObjects.Spawners.Behaviours;

public sealed class FixedPositionNonPlayerEntitySpawnerBehaviour(
    Vector2 position,
    float cooldownSeconds,
    Func<Vector2, INonPlayerEntity> entityFactory
) : ISpawnerBehaviour<INonPlayerEntity>
{
    private float _timeSinceLastSpawn = 0f;
    
    public INonPlayerEntity? TrySpawn(float deltaSeconds, Bounds stage)
    {
        _timeSinceLastSpawn += deltaSeconds;

        if (_timeSinceLastSpawn >= cooldownSeconds)
        {
            _timeSinceLastSpawn = 0f;

            return entityFactory(position);
        }

        return null;
    }
}
