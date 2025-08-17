using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Physics;

public static class MovementIntegrator
{
   public static Vector2 Advance(Vector2 pos, Direction dir, float speed, DeltaTime dt, Bounds stage)
    {
        var next = pos + dir.Vector * speed * dt;
        return stage.Clamp(next);
    }

    public static Vector2 AdvanceUnclamped(Vector2 pos, Direction dir, float speed, DeltaTime dt)
        => pos + dir.Vector * speed * dt;

}
