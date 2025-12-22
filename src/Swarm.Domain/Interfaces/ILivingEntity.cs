using Swarm.Domain.Combat;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Interfaces;

public interface ILivingEntity : ICollidable
{
    EntityId Id { get; }
    HitPoints HP { get; }
    bool IsDead { get; }
    void TakeDamage(Damage damage);
    void Heal(Damage damage);
    void Die() => TakeDamage(new(HP));

}
