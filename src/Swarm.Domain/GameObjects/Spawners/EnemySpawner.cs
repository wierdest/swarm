using Swarm.Domain.Entities;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Time;

namespace Swarm.Domain.GameObjects.Spawners;

public sealed class EnemySpawner(
    GameSession session,
    ISpawnerBehaviour<INonPlayerEntity> behaviour,
    int batchSize = 1
    ) : GameObject(session)
{
    private readonly List<INonPlayerEntity> _buffer = new();

    public override void Tick(DeltaTime dt)
    {
        var enemy = behaviour.TrySpawn(dt.Seconds, _session.Stage);

        if (enemy is null) return;

        _buffer.Add(enemy);

        if (_buffer.Count >= batchSize)
        {
            foreach (var e in _buffer)
                _session.AddEnemy(e);

            _buffer.Clear();
        }
    }
}
