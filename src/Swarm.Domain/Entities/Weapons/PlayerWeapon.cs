using Swarm.Domain.Entities.Projectiles;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities.Weapons;

public class PlayerWeapon(
    string name,
    IFirePattern pattern,
    Cooldown cooldown,
    ProjectileOwnerTypes ownerType,
    int maxAmmo
) : Weapon(pattern, cooldown, ownerType)
{
    public string Name { get; } = name;
    public int MaxAmmo { get; } = maxAmmo;
    public int CurrentAmmo { get; private set; } = maxAmmo;

    public override bool TryFire(Vector2 origin, Direction facing, out IEnumerable<Projectile> projectiles)
    {
        if (CurrentAmmo <= 0)
        {
            projectiles = [];
            return false;
        }

        var fired = base.TryFire(origin, facing, out projectiles);
        if (fired)
            CurrentAmmo--;

        return fired;
    }
    public void Reload(int availableAmmo, out int ammoUsed)
    {
        ammoUsed = 0;

        if (CurrentAmmo >= MaxAmmo || availableAmmo <= 0)
            return;

        int needed = MaxAmmo - CurrentAmmo;
        ammoUsed = Math.Min(needed, availableAmmo);

        CurrentAmmo += ammoUsed;
    }
}