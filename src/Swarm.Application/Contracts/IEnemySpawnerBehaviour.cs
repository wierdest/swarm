using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Application.Contracts;

public interface IEnemySpawnerBehaviour
{
    IEnemy? TrySpawn(float deltaSeconds, Bounds stage);
}
