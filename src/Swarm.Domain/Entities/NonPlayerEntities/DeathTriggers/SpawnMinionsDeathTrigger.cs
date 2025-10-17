using Swarm.Domain.Events;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities.NonPlayerEntities.DeathTriggers;

public sealed class SpawnMinionsDeathTrigger(
    int minionCount,
    Radius radius
) : IDeathTrigger
{
    private readonly int _minionCount = minionCount;
    private readonly Radius _radius = radius;
    
    public IEnumerable<IDomainEvent> OnDeath(Vector2 position)
    {
        var events = new List<IDomainEvent>();

        var spawnPositions = new List<Vector2>();

        for (int i = 0; i < _minionCount; i++)
        {
            float angle = MathF.Tau * i / _minionCount; // 360° around
            var offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * _radius;
            spawnPositions.Add(position + offset);
        }
        events.Add(new EnemySpawnEvent(spawnPositions));
        return events;
    }
    
}
