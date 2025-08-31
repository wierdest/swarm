using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Interfaces;

public interface ISpawnerBehaviour<out T>
{
    T? TrySpawn(float deltaSeconds, Bounds stage);

}
