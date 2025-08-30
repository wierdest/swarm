using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Interfaces;

public interface ISpawnerBehaviour
{
    IEnemy? TrySpawn(float deltaSeconds, Bounds stage);

}
