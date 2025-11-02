using Swarm.Application.Config;
using Swarm.Application.Primitives;

namespace Swarm.Application.Contracts;

public interface IGameSessionService
{
    Task StartNewSession(GameConfig config);
    bool HasSession { get; }
    void ApplyInput(float dirX, float dirY, float speed);
    void Fire(bool isPressed, bool isHeld);
    void DropBomb();
    void Reload();
    void RotateTowards(float mouseX, float mouseY, float? thumbstickRadians, float thumbstickMagnitude);
    void Pause();
    void Resume();
    Task Restart(GameConfig config);
    void Tick(float deltaSeconds);
    GameSnapshot GetSnapshot();
    Task SaveAsync(SaveName saveName, CancellationToken cancellationToken = default);
    Task LoadAllSavesAsync(SaveName saveName, CancellationToken cancellationToken = default);
    IReadOnlyList<SaveGame> GetSaveGames();
}
