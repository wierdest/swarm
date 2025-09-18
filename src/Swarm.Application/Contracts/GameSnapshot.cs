using Swarm.Application.DTOs;

namespace Swarm.Application.Contracts;

public readonly record struct GameSnapshot(
    BoundsDTO Stage,
    PlayerDTO Player,
    IReadOnlyList<ProjectileDTO> Projectiles
);