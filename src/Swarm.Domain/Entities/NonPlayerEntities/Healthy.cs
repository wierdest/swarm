using Swarm.Domain.Combat;
using Swarm.Domain.Events;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities;

public class Healthy(
    EntityId id,
    Vector2 startPosition,
    Radius radius,
    HitPoints hp,
    IDeathTrigger deathTrigger,
    INonPlayerEntityBehaviour[] behaviours)
    : NonPlayerEntityBase(id, startPosition, radius, hp, behaviours[0])
{
    private readonly IDeathTrigger _deathTrigger = deathTrigger;
    public bool IsInfected { get; private set; } = false;
    private readonly INonPlayerEntityBehaviour[] _behaviours = behaviours;
    protected override void OnDeath(NonPlayerEntityContext<INonPlayerEntity> context)
    {
        foreach (var evt in _deathTrigger.OnDeath(Position))
            RaiseEvent(evt);
    }

    protected override void ResolveCollisionWith(INonPlayerEntity other, ref Vector2 newPos, float minDist, float distSq, Vector2 delta)
    {
        if (!IsInfected && other is Zombie)
        {
            IsInfected = true;
            SwitchBehaviour(_behaviours[1]);
            RaiseEvent(new HealthyInfectedEvent(Id, Position));
            return;
        }
        base.ResolveCollisionWith(other, ref newPos, minDist, distSq, delta);
    }
}
