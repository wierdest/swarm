using Swarm.Domain.Combat;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Physics;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities;

public sealed class BasicEnemy(
    EntityId id,
    Vector2 startPosition,
    Radius radius,
    HitPoints initialHitPoints,
    IEnemyBehaviour behaviour
) : IEnemy
{
    public EntityId Id { get; } = id;

    public HitPoints HP { get; private set;  } = initialHitPoints;

    public bool IsDead => HP.IsZero;

    public Vector2 Position { get; private set; } = startPosition;

    public Radius Radius { get; } = radius;

    public bool CollidesWith(ICollidable other) => CollisionExtensions.Intersects(this, other);

    public void TakeDamage(Damage damage)
    {
        HP = HP.Take(damage.Value);
    }

    public void Tick(DeltaTime dt, Vector2 playerPosition, Bounds stage)
    {
        var movement = behaviour.DecideMovement(Position, playerPosition, dt);
        if (movement.HasValue)
            Position = MovementIntegrator.Advance(Position, movement.Value.direction, movement.Value.speed, dt, stage);
    }
}
