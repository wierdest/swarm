using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Interfaces;

public interface IEnemy : ILivingEntity
{
    void Tick(DeltaTime dt, Vector2 playerPosition, Bounds stage);
}