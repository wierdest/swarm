using Swarm.Domain.Combat;
using Swarm.Domain.Entities;

namespace Swarm.Domain.Interfaces;

public interface IGoal
{
    Score Target { get; }
    string Description { get; }
    bool Evaluate(GameSession session);
}
