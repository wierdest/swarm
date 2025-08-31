using Swarm.Domain.Combat;
using Swarm.Domain.GameObjects;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities;

public sealed class GameSession(
    EntityId id,
    Bounds stage,
    Player player,
    List<Wall> walls
)
{
    public EntityId Id { get; } = id;
    public Bounds Stage { get; } = stage;
    public Player Player { get; } = player;
    private readonly List<Projectile> _projectiles = [];
    public IReadOnlyList<Projectile> Projectiles => _projectiles;
    private readonly List<IEnemy> _enemies = [];
    public IReadOnlyList<IEnemy> Enemies => _enemies;
    private Score _score = new();
    public Score Score => _score;
    public List<Wall> Walls { get; } = walls;

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
        UpdatePlayer(dt);
        UpdateEnemies(dt);
        UpdateProjectiles(dt);
    }

    private void UpdatePlayer(DeltaTime dt)
    {
        Player.Tick(dt, Stage);
    }

    private void UpdateEnemies(DeltaTime dt)
    {
        for (int i = 0; i < _enemies.Count; i++)
        {
            var enemy = _enemies[i];
            if (enemy.IsDead)
                continue;

            // id comparison is slow, index comparison is fast, iterating plainlist also cache-ffriendly
            enemy.Tick(dt, Player.Position, Stage, _enemies, i);

            if (Player.CollidesWith(enemy))
                Player.TakeDamage(new Damage(1));
        }

        _enemies.RemoveAll(e => e.IsDead);
    }

    private void UpdateProjectiles(DeltaTime dt)
    {
        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            var projectile = _projectiles[i];
            projectile.Tick(dt);

            if (IsProjectileExpired(projectile) || HandleProjectileCollisions(projectile))
            {
                _projectiles.RemoveAt(i);
            }
        }
    }

    private bool IsProjectileExpired(Projectile projectile) => projectile.IsExpired || !Stage.Contains(projectile.Position);

    private bool HandleProjectileCollisions(Projectile projectile)
    {
        foreach (var enemy in _enemies)
        {
            if (enemy.IsDead)
                continue;

            if (projectile.CollidesWith(enemy))
            {
                enemy.TakeDamage(projectile.Damage);

                if (enemy.IsDead) {
                    _score += 1;
                }

                return true;
            }
        }
        return false;
    }
    
    public void AddEnemy(IEnemy enemy)
    {
        if (enemy != null)
            _enemies.Add(enemy);
    }
}
