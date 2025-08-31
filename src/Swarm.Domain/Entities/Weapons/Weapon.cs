using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities.Weapons;

public sealed class Weapon(
    IFirePattern pattern,
    Cooldown cooldown
)
{
    public IFirePattern Pattern { get; } = pattern;
    public Cooldown Cooldown { get; private set; } = cooldown;
    public bool TryFire(Vector2 origin, Direction facing, out IEnumerable<Projectile> projectiles)
    {
        Cooldown = Cooldown.ConsumeIfReady(out var fired);
        if (!fired)
        {
            projectiles = [];
            return false;
        }

        projectiles = Pattern.Fire(origin, facing);
        return true;
    }

    public void Tick(DeltaTime dt) => Cooldown = Cooldown.Tick(dt);
}
