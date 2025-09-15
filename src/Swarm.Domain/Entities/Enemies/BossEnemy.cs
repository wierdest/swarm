using Swarm.Domain.Combat;
using Swarm.Domain.Entities.Weapons;
using Swarm.Domain.Events;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Physics;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities.Enemies;

public class BossEnemy(
    EntityId id,
    Vector2 startPosition,
    Radius radius,
    HitPoints initialHitPoints,
    IEnemyBehaviour behaviour,
    Weapon weapon,
    IDeathTrigger deathTrigger  
) : IEnemy
{
    public EntityId Id { get; } = id;
    public HitPoints HP { get; private set; } = initialHitPoints;
    public bool IsDead => HP.IsZero;
    public Vector2 Position { get; private set; } = startPosition;
    private Vector2 _lastPosition = startPosition;
    public Radius Radius { get; } = radius;
    public Direction Rotation { get; private set; } = Direction.From(1, 0);
    private readonly IEnemyBehaviour _behaviour = behaviour;
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

    public void Tick(DeltaTime dt, Vector2 playerPosition, Bounds stage, IReadOnlyList<IEnemy> enemies, int selfIndex)
    {
        if (IsDead)
        {
            Console.WriteLine("SPAWN ENEMIESSS! 4");

            foreach (var evt in _deathTrigger.OnDeath(Position))
            {
                RaiseEvent(evt);
            }
        }

        var movement = _behaviour.DecideMovement(Position, playerPosition, dt);

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

        var toPlayer = playerPosition - Position;
        if (!toPlayer.IsZero())
        {
            Rotation = Direction.From(toPlayer.X, toPlayer.Y);
        }

        _lastPosition = Position;
        Position = newPos;
        
        ActiveWeapon.Tick(dt);

        if (_behaviour.DecideAction(Position, playerPosition, dt) &&
            ActiveWeapon.TryFire(Position, Rotation, out var projectiles))
        {

            RaiseEvent(new EnemyFiredEvent(Id, projectiles));
        }
    }
}
