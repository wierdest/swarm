namespace Swarm.Application.Contracts;

public readonly record struct GameConfig(
    StageConfig StageConfig,
    LevelConfig LevelConfig,
    float PlayerRadius,
    int RoundLength
);
