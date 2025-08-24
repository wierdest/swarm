using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Physics;

public static class CollisionExtensions
{
    public static bool Intersects(this ICollidable a, ICollidable b)
    {
        var diff = a.Position - b.Position;
        var radiusSum = a.Radius.Value + b.Radius.Value;
        return diff.LengthSquared() <= radiusSum * radiusSum;
    }

    public static bool Intersects(this ICollidable a, Vector2 proposedPosition, ICollidable b)
    {
        var diff = proposedPosition - b.Position;
        var radiusSum = a.Radius.Value + b.Radius.Value;
        return diff.LengthSquared() <= radiusSum * radiusSum;
    }
}
