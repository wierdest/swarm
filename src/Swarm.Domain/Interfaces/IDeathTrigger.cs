using System.Numerics;
using Swarm.Domain.Time;

namespace Swarm.Domain.Interfaces;

public interface IDeathTrigger
{
    void OnDeath(DeltaTime dt, Vector2 position);
}
