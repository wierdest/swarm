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

    protected override void ResolveCollisionWith(INonPlayerEntity other, ref Vector2 newPos, float minDist, float distSq, Vector2 delta)
    {
        if (other is Zombie)
        {
            Die();
            return;
        }
        base.ResolveCollisionWith(other, ref newPos, minDist, distSq, delta);
    }


    
}
