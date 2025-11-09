using Swarm.Domain.Combat;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities;

public class Healthy(
    EntityId id,
    Vector2 startPosition,
    Radius radius,
    HitPoints hp,
    IDeathTrigger deathTrigger,
    INonPlayerEntityBehaviour behaviour)
    : NonPlayerEntityBase(id, startPosition, radius, hp, behaviour)
{
    private readonly IDeathTrigger _deathTrigger = deathTrigger;

    protected override void OnDeath(NonPlayerEntityContext<INonPlayerEntity> context)
    {
        foreach (var evt in _deathTrigger.OnDeath(Position))
            DomainEventList.Add(evt);
    }

    public override void Tick(NonPlayerEntityContext<INonPlayerEntity> context)
    {
        base.Tick(context);
        if (IsDead) return;

        // decide if will check the proximity with any Radicals, 
        // in which case it would die and trigger the spawning of
        // another Radical.
        // this is called 'conversion'.


    }
    
}
