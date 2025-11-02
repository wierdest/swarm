using Swarm.Application.DTOs;

namespace Swarm.Application.Contracts;

public record class SaveGame(
    DateTimeOffset SaveTime,
    Hud Hud,
    IEnumerable<BombDTO> Bombs
);
