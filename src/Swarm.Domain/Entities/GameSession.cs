using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities;

public sealed class GameSession(
    Bounds stage,
    Player player
)
{
    public Bounds Stage { get; } = stage;
    public Player Player { get; } = player;
    private readonly List<Projectile> _projectiles = [];
    public IReadOnlyList<Projectile> Projectiles => _projectiles;
    public void ApplyInput(Direction dir, float speed) =>
        Player.ApplyInput(dir, speed);

    public void Fire()
    {
        if (Player.TryFire(out var projectiles))
            _projectiles.AddRange(projectiles);
    }

    public void RotatePlayerTowards(Vector2 target) =>
        Player.RotateTowards(target);
    
    public void Tick(DeltaTime dt)
    {
        Player.Tick(dt, Stage);

        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            var p = _projectiles[i];
            p.Tick(dt);

            if (p.IsExpired || !Stage.Contains(p.Position))
                _projectiles.RemoveAt(i);
        }
    }

}
