using Swarm.Domain.Entities;
using Swarm.Domain.Entities.Projectiles;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Interfaces;

public interface IFirePattern
{
    IEnumerable<Projectile> Fire(Vector2 origin, Direction facing, ProjectileOwnerTypes ownerType);
}
