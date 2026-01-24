using Swarm.Application.Config;

namespace Swarm.Application.Contracts;

public interface IGameSessionConfigLoader
{
    GameSessionConfig Load(string json);
}
