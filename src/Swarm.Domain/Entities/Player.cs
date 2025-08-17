using Swarm.Domain.Physics;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities;

public sealed class Player(
    EntityId id,
    Vector2 startPos,
    Radius radius,
    Weapon weapon
)
{
    public EntityId Id { get; } = id;
    public Vector2 Position { get; private set; } = startPos;
    public Radius Radius { get; } = radius;
    public Weapon ActiveWeapon { get; private set; } = weapon;
    public Direction Direction { get; private set; } = Direction.From(1, 0);
    public float Speed { get; private set; } = 0f;
    public void ApplyInput(Direction dir, float speed)
    {
        Direction = dir;
        Speed = speed;
    }

    public bool TryFire(out IEnumerable<Projectile> projectiles) =>
        ActiveWeapon.TryFire(Position, out projectiles);

    public void Tick(DeltaTime dt, Bounds stage)
    {
        ActiveWeapon.Tick(dt);
        Position = MovementIntegrator.Advance(Position, Direction, Speed, dt, stage);
    }
}
