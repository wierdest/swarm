using Swarm.Application.DTOs;

namespace Swarm.Application.Contracts;

public record class SaveGame(
    DateTimeOffset SaveTime,
    HudData HudData,
    IEnumerable<BombDTO> Bombs
);
