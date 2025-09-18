using Swarm.Domain.Combat;
using Swarm.Domain.Entities.Weapons;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Physics;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities;

public sealed class Player(
    EntityId id,
    Vector2 startPos,
    Radius radius,
    PlayerWeapon weapon,
    int initialAmmo = 0
) : ILivingEntity
{
    public EntityId Id { get; } = id;
    public Vector2 Position { get; private set; } = startPos;
    private Vector2 _lastPosition = startPos;  
    public Radius Radius { get; } = radius;
    public PlayerWeapon ActiveWeapon { get; private set; } = weapon;
    public Direction Direction { get; private set; } = Direction.From(1, 0);
    public Direction Rotation { get; private set; } = Direction.From(1, 0);
    public float Speed { get; private set; } = 0f;
    public HitPoints HP { get; private set; } = new(100);
    public int Ammo { get; private set; } = initialAmmo;
    public bool IsDead => HP.IsZero;

    public void ReloadWeapon()
    {
        ActiveWeapon.Reload(Ammo, out var ammoUsed);
        Ammo -= ammoUsed;
    }

    public void AddAmmo(int amount)
    {
        if (amount > 0)
            Ammo += amount;
    }

    public void RotateTowards(Vector2 target)
    {
        var lookDir = target - Position;

        if (!lookDir.IsZero())
            Rotation = Direction.From(lookDir.X, lookDir.Y);
    }
    public void ApplyInput(Direction dir, float speed)
    {
        Direction = dir;
        Speed = speed;
    }

    public bool TryFire(out IEnumerable<Projectile> projectiles) =>
        ActiveWeapon.TryFire(Position, Rotation, out projectiles);

    public void Tick(DeltaTime dt, Bounds stage)
    {
        ActiveWeapon.Tick(dt);
        _lastPosition = Position;
        Position = MovementIntegrator.Advance(Position, Direction, Speed, dt, stage);
    }

    public void RevertLastMovement()
    {
        Position = _lastPosition;
    }

    public void TakeDamage(Damage damage)
    {
        HP = HP.Take(damage);
    }

    public bool CollidesWith(ICollidable other) => CollisionExtensions.Intersects(this, other);

    public void Heal(Damage damage)
    {
        if (HP >= 100) return;
        HP = HP.Heal(damage);
    }

    public void Respawn(Vector2 position)
    {
        Position = position;
        _lastPosition = position;
    }
}
