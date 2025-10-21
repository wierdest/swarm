using Swarm.Domain.Interfaces;
using Swarm.Domain.Physics;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.GameObjects;

public sealed class Wall(Vector2 position, Radius radius, IEnumerable<Vector2>? spawners) : ICollidable
{
    public Vector2 Position { get; } = position;
    public Radius Radius { get; } = radius;
    public IEnumerable<Vector2>? Spawners = spawners;
    public bool CollidesWith(ICollidable other) => CollisionExtensions.Intersects(this, other);
}
