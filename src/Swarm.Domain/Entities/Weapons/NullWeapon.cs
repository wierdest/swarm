using Swarm.Domain.Entities.Projectiles;
using Swarm.Domain.Entities.Weapons.Patterns;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities.Weapons;

public sealed class NullWeapon : Weapon
{
    public NullWeapon() : base(new NoFirePattern(), Cooldown.AlwaysReady, ProjectileOwnerTypes.None) { }

    public override bool TryFire(Vector2 origin, Direction facing, out IEnumerable<Projectile> projectiles)
    {
        projectiles = [];
        return false;
    }
}
