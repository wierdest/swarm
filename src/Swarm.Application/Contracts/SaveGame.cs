using Swarm.Application.Contracts;

namespace Swarm.Application;

public record class SaveGame(
    DateTimeOffset SaveTime,
    Hud Hud
);
