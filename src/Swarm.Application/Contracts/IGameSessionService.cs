using Swarm.Application.Config;

namespace Swarm.Application.Contracts;

public interface IGameSessionService
{
    Task StartNewSession(GameSessionConfig config);
    bool HasSession { get; }
    void ApplyInput(float dirX, float dirY, float speed);
    void Fire(bool isPressed, bool isHeld);
    void DropBomb();
    void Reload();
    void RotateTowards(float mouseX, float mouseY, float? thumbstickRadians, float thumbstickMagnitude);
    void Pause();
    void Resume();
    Task Restart(GameSessionConfig config);
    void Tick(float deltaSeconds);
    GameSnapshot GetSnapshot();
}
