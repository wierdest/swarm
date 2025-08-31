namespace Swarm.Application.Contracts;

public readonly record struct SpawnerConfig(
    float X,
    float Y,
    string BehaviourType,
    float CooldownSeconds
);
