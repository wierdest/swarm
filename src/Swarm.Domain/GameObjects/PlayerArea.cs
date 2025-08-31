using Swarm.Domain.Combat;
using Swarm.Domain.Entities;
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

    private readonly Damage _standardHealAmount = new(1);

    public override void Tick(DeltaTime dt)
    {
        var player = _session.Player;

        if (player.IsDead)
        {
            player.Respawn(Position);
        }

        if (IsInside(player.Position))
        {
            player.Heal(_standardHealAmount);
        }

        foreach (var projectile in _session.Projectiles)
        {
            if (IsInside(projectile.Position))
            {
                projectile.Expire();
            }
        }
    }
    private bool IsInside(Vector2 point)
    {
        return Vector2.DistanceSquared(point, Position) <= Radius.Value * Radius.Value;
    }
}