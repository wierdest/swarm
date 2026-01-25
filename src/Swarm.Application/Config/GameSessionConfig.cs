namespace Swarm.Application.Config;

public sealed record class GameSessionConfig(
    StageConfig StageConfig,
    LevelConfig LevelConfig,
    float PlayerRadius,
    int RoundLength
);
