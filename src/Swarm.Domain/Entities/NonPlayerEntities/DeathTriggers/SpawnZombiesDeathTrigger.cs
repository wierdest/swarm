using Swarm.Domain.Events;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities.DeathTriggers;

public sealed class SpawnZombiesDeathTrigger(
    int count,
    Radius radius
) : IDeathTrigger
{
    private readonly int _count = count;
    private readonly Radius _radius = radius;
    
    public IEnumerable<IDomainEvent> OnDeath(Vector2 position)
    {
        var events = new List<IDomainEvent>();

        var spawnPositions = new List<Vector2>();

        for (int i = 0; i < _count; i++)
        {
            float angle = MathF.Tau * i / _count; // 360°
            var offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * _radius;
            spawnPositions.Add(position + offset);
        }
        events.Add(new RadicalSpawnEvent(spawnPositions));
        return events;
    }
    
}
