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

    public Direction Rotation { get; private set; } = Direction.From(1, 0);

    public bool CollidesWith(ICollidable other) => CollisionExtensions.Intersects(this, other);

    public void TakeDamage(Damage damage)
    {
        HP = HP.Take(damage.Value);
    }

    public void Tick(DeltaTime dt, Vector2 playerPosition, Bounds stage, IReadOnlyList<IEnemy> enemies, int selfIndex)
    {
        var movement = behaviour.DecideMovement(Position, playerPosition, dt);

        if (!movement.HasValue) return;

        var newPos = MovementIntegrator.Advance(Position, movement.Value.direction, movement.Value.speed, dt, stage);

        for (int i = 0; i < enemies.Count; i++)
        {
            if (i == selfIndex) continue;

            var other = enemies[i];
            if (other.IsDead) continue;

            if (CollisionExtensions.Intersects(this, newPos, other))
            {
                // simple bounce
                var away = (Position - other.Position).Normalized();
                newPos += away * 0.1f;
            }
        }

        Rotation = Rotation.Rotated(MathF.PI * dt.Seconds);
        
        Position = newPos;
    }
}
