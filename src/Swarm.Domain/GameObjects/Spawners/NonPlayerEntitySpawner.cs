using Swarm.Domain.Entities;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Time;

namespace Swarm.Domain.GameObjects.Spawners;

public sealed class NonPlayerEntitySpawner(
    GameSession session,
    ISpawnerBehaviour<INonPlayerEntity> behaviour,
    int batchSize = 1
    ) : GameObject(session)
{
    private readonly List<INonPlayerEntity> _buffer = new();

    public override void Tick(DeltaTime dt)
    {
        if (_session.ReachedMaxNonPlayerEntities) return;
        
        var nonPlayerEntity = behaviour.TrySpawn(dt.Seconds, _session.Stage);

        if (nonPlayerEntity is null) return;

        _buffer.Add(nonPlayerEntity);

        if (_buffer.Count >= batchSize)
        {
            foreach (var e in _buffer)
                _session.AddNonPlayerEntity(e);

            _buffer.Clear();
        }
    }
}
