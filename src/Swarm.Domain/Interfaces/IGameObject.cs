using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Interfaces;

public interface IGameObject
{
    void Tick(DeltaTime dt);
    Vector2 Position { get; }
}
