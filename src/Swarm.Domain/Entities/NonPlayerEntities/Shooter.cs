using Swarm.Domain.Combat;
using Swarm.Domain.Entities.Weapons;
using Swarm.Domain.Events;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities;

public sealed class Shooter(
    EntityId id,
    Vector2 startPosition,
    Radius radius,
    HitPoints hp,
    INonPlayerEntityBehaviour behaviour,
    Weapon weapon,
    IDeathTrigger deathTrigger
) : NonPlayerEntityBase(id, startPosition, radius, hp, behaviour)
{
    private readonly IDeathTrigger _deathTrigger = deathTrigger;
    private readonly Weapon _weapon = weapon;

    protected override void UpdateRotation(NonPlayerEntityContext<INonPlayerEntity> context)
    {
        var toPlayer = context.PlayerPosition - Position;
        if (!toPlayer.IsZero())
            Rotation = Direction.From(toPlayer.X, toPlayer.Y);
    }

    protected override void OnDeath(NonPlayerEntityContext<INonPlayerEntity> context)
    {
        foreach (var evt in _deathTrigger.OnDeath(Position))
            RaiseEvent(evt);
    }

    public override void Tick(NonPlayerEntityContext<INonPlayerEntity> context)
    {
        base.Tick(context);
        if (IsDead) return;

        _weapon.Tick(context.DeltaTime);

        if (Behaviour.DecideAction(context) &&
            _weapon.TryFire(Position, Rotation, out var projectiles))
        {
            RaiseEvent(new EnemyFiredEvent(Id, projectiles));
        }
    }
}
