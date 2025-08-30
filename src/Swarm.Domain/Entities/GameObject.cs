using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities;

public abstract class GameObject(GameSession session) : IGameObject
{
    protected readonly GameSession _session = session;
    public abstract Vector2 Position { get; }
    public abstract void Tick(DeltaTime dt);

}
