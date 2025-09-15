using Swarm.Domain.Entities.Projectiles;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.Weapons.Patterns;

public sealed class NoFirePattern : IFirePattern
{
    public IEnumerable<Projectile> Fire(Vector2 origin, Direction facing, ProjectileOwnerTypes ownerType) => [];

}

