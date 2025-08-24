using Swarm.Domain.Combat;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Interfaces;

public interface ILivingEntity : ICollidable
{
    EntityId Id { get; }
    HitPoints HP { get; }
    bool IsDead { get; }
    void TakeDamage(Damage damage);

}
