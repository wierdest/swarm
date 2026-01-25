using Swarm.Domain.Combat;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities;

public sealed class Zombie : NonPlayerEntityBase
{
    private readonly INonPlayerEntityBehaviour _behaviour;

    public Zombie(
        EntityId id,
        Vector2 startPosition,
        Radius radius,
        HitPoints hp,
        INonPlayerEntityBehaviour behaviour)
        : base(id, startPosition, radius, hp, behaviour)
    {
        _behaviour = behaviour;
    }

    public INonPlayerEntityBehaviour GetBehaviour() => _behaviour;
}
