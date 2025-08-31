using Swarm.Domain.Entities;
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

    public override void Tick(DeltaTime dt)
    {
        var player = _session.Player;

        if (!player.IsDead && IsInside(player.Position))
        {
            _session.CompleteLevel();
        }
    }
    
    private bool IsInside(Vector2 point)
    {
        return Vector2.DistanceSquared(point, Position) <= Radius.Value * Radius.Value;
    }
}