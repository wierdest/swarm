namespace Swarm.Application.Config;

public sealed record class WallGeneratorConfig(
    int? WallRandomSeed, // this number will store the seed used in the random
    int WallSeedCount, // number of seeds used to generate walls
    float WallCellSize, // grid sampling size for Voronoi walls
    float WallRadius, // radius for generated wall objects
    float WallDensity, // chance to place a wall on a Voronoi boundary
    float? CorridorWidthMultiplier // multiplier for guaranteed corridor width
);

