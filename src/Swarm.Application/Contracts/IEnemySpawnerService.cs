using Swarm.Domain.Time;

namespace Swarm.Application.Contracts;

public interface IEnemySpawnerService
{
    void Tick(DeltaTime dt);
}
