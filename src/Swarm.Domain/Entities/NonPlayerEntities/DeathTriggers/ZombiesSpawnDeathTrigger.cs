using Swarm.Domain.Combat;
using Swarm.Domain.Events;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities.DeathTriggers;

public sealed class ZombiesSpawnDeathTrigger(
    int count,
    Radius radius,
    HitPoints hp,
    float speed,
    float? targetThreshold,
    float? dodgeThreshold,
    float? dodgeMultiplier
) : IDeathTrigger
{
    private readonly int _count = count;
    private readonly Radius _radius = radius;
    private readonly HitPoints _hp = hp;
    private readonly float _speed = speed;
    private readonly float? _targetThreshold = targetThreshold;
    private readonly float? _dodgeThreshold = dodgeThreshold;
    private readonly float? _dodgeMultiplier = dodgeMultiplier;
    
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
        events.Add(new ZombieSpawnEvent(
            spawnPositions,
            _radius,
            _hp,
            _speed,
            _targetThreshold,
            _dodgeThreshold,
            _dodgeMultiplier
        ));
        return events;
    }
    
}
