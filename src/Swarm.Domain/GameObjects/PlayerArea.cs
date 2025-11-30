using Swarm.Domain.Combat;
using Swarm.Domain.Entities;
using Swarm.Domain.Entities.NonPlayerEntities;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.GameObjects;

public sealed class PlayerArea(
    GameSession session,
    Vector2 position,
    Radius radius
) : GameObject(session)
{
    public Vector2 Position { get; } = position;
    public Radius Radius { get; } = radius;
    private int _playerRespawns = 0;
    public int PlayerRespawns => _playerRespawns;
    private readonly Damage _standardHealAmount = new(1);

    public override void Tick(DeltaTime dt)
    {
        var player = _session.Player;

        if (player.IsDead)
        {
            player.Respawn(Position);
            _playerRespawns++;
        }

        if (IsInside(player.Position))
        {
            player.Heal(_standardHealAmount);
            player.AddAmmo(1);

        }
        
        foreach (var projectile in _session.Projectiles)
        {
            if (IsInside(projectile.Position))
            {
                projectile.Expire();
            }
        }

        foreach (var entity in _session.NonPlayerEntities)
        {
            if (IsInside(entity.Position))
            {
                if (entity is Healthy)
                {
                    _session.SaveHealthy(entity);
                    return;
                }
                entity.RevertLastMovement();
            }
        }
    }
    private bool IsInside(Vector2 point)
    {
        return Vector2.DistanceSquared(point, Position) <= Radius.Value * Radius.Value;
    }
}