namespace Swarm.Domain.Entities.Missions.Goals;

public sealed class KillsGoal(int targetValue) : Goal
{
    private readonly int _targetValue = targetValue;

    public override string Description => $"Kill {_targetValue} enemies!";

    public override bool EvaluateAsComplete(GameSession session) => session.Kills >= _targetValue;
}
