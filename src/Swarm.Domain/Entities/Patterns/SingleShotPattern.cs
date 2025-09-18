using Swarm.Domain.Combat;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.Patterns;

public sealed class SingleShotPattern(
    Damage damage,
    float projectileSpeed,
    Radius projectileRadius,
    float lifetime = 1.5f
) : IFirePattern
{
    public IEnumerable<Projectile> Fire(Vector2 origin, Direction facing)
    {
        yield return new Projectile(
            EntityId.New(),
            origin,
            facing,
            projectileSpeed,
            projectileRadius,
            damage,
            lifetime
        );
    }
}
