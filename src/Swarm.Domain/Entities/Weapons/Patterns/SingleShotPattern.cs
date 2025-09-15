using Swarm.Domain.Combat;
using Swarm.Domain.Entities.Projectiles;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.Weapons.Patterns;

public sealed class SingleShotPattern(
    Damage damage,
    float projectileSpeed,
    Radius projectileRadius,
    float lifetime = 1.5f
) : IFirePattern
{
    public IEnumerable<Projectile> Fire(Vector2 origin, Direction facing, ProjectileOwnerTypes ownerType)
    {
        var motionData = new ProjectileMotionData(origin, facing, projectileSpeed);
        yield return new Projectile(
            EntityId.New(),
            motionData,
            projectileRadius,
            damage,
            lifetime,
            ownerType
        );
    }
}
