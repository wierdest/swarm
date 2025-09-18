using Swarm.Domain.Combat;
using Swarm.Domain.Common;
using Swarm.Domain.Entities.Projectiles;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Physics;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities;

public sealed class Projectile(
    EntityId id,
    ProjectileMotionData motionData,
    Radius radius,
    Damage damage,
    float lifetime,
    ProjectileOwnerTypes owner
) : ICollidable
{
    public EntityId Id { get; } = id;
    public Vector2 Position { get; private set; } = motionData.Position;
    public Direction Direction { get; } = motionData.Direction;
    public float Speed { get; } = motionData.Speed;
    public ProjectileOwnerTypes Owner { get; } = owner;
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

    public void Expire()
    {
        LifetimeRemaining = 0f;
    }
}
