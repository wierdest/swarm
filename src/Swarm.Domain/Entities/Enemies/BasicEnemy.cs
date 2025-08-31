using Swarm.Domain.Combat;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Physics;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities.Enemies;

public sealed class BasicEnemy(
    EntityId id,
    Vector2 startPosition,
    Radius radius,
    HitPoints initialHitPoints,
    IEnemyBehaviour behaviour
) : IEnemy
{
    public EntityId Id { get; } = id;

    public HitPoints HP { get; private set; } = initialHitPoints;

    public bool IsDead => HP.IsZero;

    public Vector2 Position { get; private set; } = startPosition;

    private Vector2 _lastPosition = startPosition;

    public Radius Radius { get; } = radius;

    public Direction Rotation { get; private set; } = Direction.From(1, 0);

    public bool CollidesWith(ICollidable other) => CollisionExtensions.Intersects(this, other);

    public void Heal(Damage damage)
    {
        throw new NotImplementedException();
    }

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

            float minDist = Radius.Value + other.Radius.Value;
            var delta = newPos - other.Position;
            float distSq = delta.LengthSquared();

            if (distSq < minDist * minDist)
            {
                float dist = MathF.Sqrt(distSq);

                var pushDir = dist > 1e-8f ? delta / dist : Rotation.Vector;

                float overlap = minDist - dist;

                newPos += pushDir * overlap;
            }
        }

        Rotation = Rotation.Rotated(MathF.PI * dt.Seconds);

        _lastPosition = Position;

        Position = newPos;
    }
    
    public void RevertLastMovement()
    {
        Position = _lastPosition;
    }
}
