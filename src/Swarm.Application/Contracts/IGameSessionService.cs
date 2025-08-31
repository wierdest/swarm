namespace Swarm.Application.Contracts;

public interface IGameSessionService
{
    void StartNewSession(GameConfig config);
    void ApplyInput(float dirX, float dirY, float speed);
    void Stop();
    void Fire();
    void RotateTowards(float targetX, float targetY);
    void Tick(float deltaSeconds);
    GameSnapshot GetSnapshot();
}
