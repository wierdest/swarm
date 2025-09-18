using Swarm.Application.Config;
using Swarm.Application.Primitives;

namespace Swarm.Application.Contracts;

public interface IGameSessionService
{
    void StartNewSession(GameConfig config);
    void ApplyInput(float dirX, float dirY, float speed);
    void Stop();
    void Fire();
    void Reload();
    void RotateTowards(float targetX, float targetY);
    void Pause();
    void Resume();
    void Tick(float deltaSeconds);
    GameSnapshot GetSnapshot();
    Task SaveAsync(SaveName saveName, CancellationToken cancellationToken = default);
    Task<GameSnapshot?> LoadAsync(SaveName saveName, CancellationToken cancellationToken = default);
}
