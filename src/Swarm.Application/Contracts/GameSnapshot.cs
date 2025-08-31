using Swarm.Application.DTOs;

namespace Swarm.Application.Contracts;

public readonly record struct GameSnapshot( 
    BoundsDTO Stage,
    PlayerDTO Player,
    HudDTO Hud,
    IReadOnlyList<ProjectileDTO> Projectiles,
    IReadOnlyList<EnemyDTO> Enemies,
    IReadOnlyCollection<DrawableDTO> Walls,
    DrawableDTO PlayerArea,
    DrawableDTO TargetArea
);