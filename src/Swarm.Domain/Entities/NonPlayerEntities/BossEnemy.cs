using Swarm.Domain.Combat;
using Swarm.Domain.Entities.Weapons;
using Swarm.Domain.Events;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Physics;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities;

public class BossEnemy(
    EntityId id,
    Vector2 startPosition,
    Radius radius,
    HitPoints initialHitPoints,
    INonPlayerEntityBehaviour behaviour,
    Weapon weapon,
    IDeathTrigger deathTrigger
) : INonPlayerEntity
{
    public EntityId Id { get; } = id;
    public HitPoints HP { get; private set; } = initialHitPoints;
    public bool IsDead => HP.IsZero;
    public Vector2 Position { get; private set; } = startPosition;
    private Vector2 _lastPosition = startPosition;
    public Radius Radius { get; } = radius;
    public Direction Rotation { get; private set; } = Direction.From(1, 0);
    private readonly INonPlayerEntityBehaviour _behaviour = behaviour;
    private readonly IDeathTrigger _deathTrigger = deathTrigger;
    public Weapon ActiveWeapon { get; private set; } = weapon;

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
    private void RaiseEvent(IDomainEvent evt) => _domainEvents.Add(evt);
    public void ClearDomainEvents() => _domainEvents.Clear();

    public bool CollidesWith(ICollidable other) => CollisionExtensions.Intersects(this, other);

    public void Heal(Damage damage)
    {
        HP = HP.Heal(damage);
    }

    public void RevertLastMovement()
    {
        Position = _lastPosition;
    }

    public void TakeDamage(Damage damage)
    {
        HP = HP.Take(damage);
    }

    public void Tick(NonPlayerEntityContext context)
    {
        if (IsDead)
        {
            foreach (var evt in _deathTrigger.OnDeath(Position))
            {
                RaiseEvent(evt);
            }
        }

        var movement = _behaviour.DecideMovement(context);

        if (!movement.HasValue) return;

        var newPos = MovementIntegrator.Advance(Position, movement.Value.direction, movement.Value.speed, context.DeltaTime, context.Stage);

        var enemies = context.Enemies;

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

        _lastPosition = Position;
        Position = newPos;

        LookTowardsPlayer(context.PlayerPosition);
        Fire(context);
    }
    
    private void LookTowardsPlayer(Vector2 playerPosition)
    {
        var toPlayer = playerPosition - Position;
        if (!toPlayer.IsZero())
        {
            Rotation = Direction.From(toPlayer.X, toPlayer.Y);
        }
    }

    private void Fire(NonPlayerEntityContext context)
    {
        ActiveWeapon.Tick(context.DeltaTime);

        if (_behaviour.DecideAction(context) &&
            ActiveWeapon.TryFire(Position, Rotation, out var projectiles))
        {
            RaiseEvent(new EnemyFiredEvent(Id, projectiles));
        }
    }
}
