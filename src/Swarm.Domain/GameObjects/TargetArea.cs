using Swarm.Domain.Combat;
using Swarm.Domain.Entities;
using Swarm.Domain.Entities.NonPlayerEntities;
using Swarm.Domain.Entities.Projectiles;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.GameObjects;

public sealed class TargetArea(
    GameSession session,
    Vector2 position,
    Radius radius
) : GameObject(session)
{
    public Vector2 Position { get; } = position;
    public Radius Radius { get; } = radius;
    private readonly Damage _standardHealAmount = new(1);
    private bool _isOpenToPlayer = false;
    public void OpenToPlayer() => _isOpenToPlayer = true;
    public bool IsOpenToPlayer => _isOpenToPlayer; 

    public override void Tick(DeltaTime dt)
    {
        var player = _session.Player;

        if (IsInside(player.Position) && !_isOpenToPlayer)
        {
            player.RevertLastMovement();
        }

        if (!player.IsDead && IsInside(player.Position))
        {
            _session.CompleteLevel();
        }

        foreach (var projectile in _session.Projectiles)
        {
            if (projectile.Owner.Equals(ProjectileOwnerTypes.Enemy)) continue;

            if (IsInside(projectile.Position))
            {
                projectile.Expire();
            }
        }

        foreach (var enemy in _session.NonPlayerEntities)
        {

            if (IsInside(enemy.Position))
            {
                if (enemy is BossEnemy)
                {
                    enemy.Heal(_standardHealAmount);
                    continue;
                }
            }
        }
    }

    private bool IsInside(Vector2 point)
    {
        return Vector2.DistanceSquared(point, Position) <= Radius.Value * Radius.Value;
    }
}