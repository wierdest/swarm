using Swarm.Domain.Combat;
using Swarm.Domain.Common;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Physics;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities;

public sealed class Projectile(
    EntityId id,
    Vector2 position,
    Direction direction,
    float speed,
    Radius radius,
    Damage damage,
    float lifetime
) : ICollidable
{
    public EntityId Id { get; } = id;
    public Vector2 Position { get; private set; } = position;
    public Direction Direction { get; } = direction;
    public float Speed { get; } = speed;
    public Radius Radius { get; } = radius;
    public Damage Damage { get; } = damage;
    public float LifetimeRemaining { get; private set; } = GuardedLifetime(lifetime);

    private static float GuardedLifetime(float lifetime)
    {
        Guard.Positive(lifetime, nameof(lifetime));
        return lifetime;
    }

    public void Tick(DeltaTime dt)
    {
        Position = MovementIntegrator.AdvanceUnclamped(Position, Direction, Speed, dt);
        LifetimeRemaining -= dt;
    }

    public bool CollidesWith(ICollidable other) => CollisionExtensions.Intersects(this, other);
    public bool IsExpired => LifetimeRemaining <= 0f;
}
