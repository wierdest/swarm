using Swarm.Domain.Combat;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities;

public sealed class Zombie(
    EntityId id,
    Vector2 startPosition,
    Radius radius,
    HitPoints hp,
    INonPlayerEntityBehaviour behaviour)
    : NonPlayerEntityBase(id, startPosition, radius, hp, behaviour)
{
}