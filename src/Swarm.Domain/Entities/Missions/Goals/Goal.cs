namespace Swarm.Domain.Entities.Missions.Goals;

public abstract class Goal
{
    public abstract string Description { get; }
    public abstract bool EvaluateAsComplete(GameSession session);
}
