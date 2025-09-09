using Swarm.Application.DTOs;

namespace Swarm.Application.Contracts;

public record class GameSnapshot( 
    BoundsDTO Stage,
    PlayerDTO Player,
    HudDTO Hud,
    IReadOnlyList<ProjectileDTO> Projectiles,
    IReadOnlyList<EnemyDTO> Enemies,
    IReadOnlyCollection<DrawableDTO> Walls,
    DrawableDTO PlayerArea,
    DrawableDTO TargetArea,
    bool IsPaused
);