namespace Swarm.Application.Contracts;


public readonly record struct StageConfig(
    float Left,
    float Top,
    float Right,
    float Bottom,
    float PlayerStartX,
    float PlayerStartY,
    float PlayerRadius,
    WeaponConfig Weapon
);
