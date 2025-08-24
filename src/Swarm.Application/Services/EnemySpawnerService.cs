using Swarm.Application.Contracts;
using Swarm.Domain.Entities;
using Swarm.Domain.Time;

namespace Swarm.Application.Services;

public sealed class EnemySpawnerService(
    GameSession session,
    IEnemySpawnerBehaviour spawnBehaviour
) : IEnemySpawnerService
{
    private readonly GameSession _session = session;
    private readonly IEnemySpawnerBehaviour _spawnBehaviour = spawnBehaviour;

    public void Tick(DeltaTime dt)
    {
        var enemy = _spawnBehaviour.TrySpawn(dt.Seconds, _session.Stage);
        if (enemy is not null)
            _session.AddEnemy(enemy);
    }

}
