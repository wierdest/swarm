namespace Swarm.Application.Config;

public sealed record class GameConfig(
    StageConfig StageConfig,
    LevelConfig LevelConfig,
    float PlayerRadius,
    int RoundLength
);
