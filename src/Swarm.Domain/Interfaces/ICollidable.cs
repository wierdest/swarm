using Swarm.Domain.Physics;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Interfaces;

public interface ICollidable
{
    Vector2 Position { get; }
    Radius Radius { get; }
    bool CollidesWith(ICollidable other);

}