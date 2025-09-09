using Swarm.Domain.Entities;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Time;

namespace Swarm.Domain.GameObjects;

public abstract class GameObject(GameSession session) : IGameObject
{
    protected readonly GameSession _session = session;
    public abstract void Tick(DeltaTime dt);
        
}
