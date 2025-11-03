using Swarm.Domain.Combat;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Physics;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities;

public abstract class NonPlayerEntityBase(
    EntityId id,
    Vector2 startPosition,
    Radius radius,
    HitPoints hp,
    INonPlayerEntityBehaviour behaviour
) : INonPlayerEntity
{
    public EntityId Id { get; } = id;
    public HitPoints HP { get; protected set; } = hp;
    public bool IsDead => HP.IsZero;
    public Vector2 Position { get; protected set; } = startPosition;
    protected Vector2 LastPosition { get; private set; } = startPosition;
    public Radius Radius { get; } = radius;
    public Direction Rotation { get; protected set; } = Direction.From(1, 0);
    protected readonly INonPlayerEntityBehaviour Behaviour = behaviour;
    protected readonly List<IDomainEvent> DomainEventList = new();
    public virtual IReadOnlyList<IDomainEvent>? DomainEvents => DomainEventList;

     public virtual void Heal(Damage damage)
    {
        HP = HP.Heal(damage);
    }

    public virtual void TakeDamage(Damage damage)
    {
        HP = HP.Take(damage);
    }

    public virtual void ClearDomainEvents() => DomainEventList.Clear();

    public bool CollidesWith(ICollidable other) => CollisionExtensions.Intersects(this, other);

    public virtual void Tick(NonPlayerEntityContext context)
    {
        if (IsDead)
        {
            OnDeath(context);
            return;
        }

        var movement = Behaviour.DecideMovement(context);
        if (!movement.HasValue) return;

        var newPos = MovementIntegrator.Advance(Position, movement.Value.direction, movement.Value.speed, context.DeltaTime, context.Stage);

        AvoidOverlap(context, ref newPos);

        LastPosition = Position;
        Position = newPos;

        UpdateRotation(context);
    }

    protected virtual void UpdateRotation(NonPlayerEntityContext context)
    {
        Rotation = Rotation.Rotated(MathF.PI * context.DeltaTime);
    }

    protected virtual void AvoidOverlap(NonPlayerEntityContext context, ref Vector2 newPos)
    {
        var enemies = context.Others;
        var selfIndex = context.SelfIndex;

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
    }

    public void RevertLastMovement() => Position = LastPosition;

    protected virtual void OnDeath(NonPlayerEntityContext context) { }
}