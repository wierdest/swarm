namespace Swarm.Application.Config;

public sealed record class GoalConfig(
    string GoalDescription,
    string GameSessionPropertyIdentifier,
    string Operator,
    int TargetValue
);

