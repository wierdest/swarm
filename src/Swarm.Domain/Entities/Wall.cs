using Swarm.Domain.Interfaces;
using Swarm.Domain.Physics;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities;

public sealed class Wall(Vector2 position, Radius radius) : ICollidable
{
    public Vector2 Position { get; } = position;
    public Radius Radius { get; } = radius;
    public bool CollidesWith(ICollidable other) => CollisionExtensions.Intersects(this, other);
}
