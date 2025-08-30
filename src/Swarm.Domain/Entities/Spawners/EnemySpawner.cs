using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities.Spawners;

public sealed class EnemySpawner(
    GameSession session,
    Vector2 position,
    ISpawnerBehaviour behaviour
    ) : GameObject(session)
{
    public override Vector2 Position { get; } = position;

    public override void Tick(DeltaTime dt)
    {
         var enemy = behaviour.TrySpawn(dt.Seconds, _session.Stage);
        if (enemy is not null)
            _session.AddEnemy(enemy);
    }
}
