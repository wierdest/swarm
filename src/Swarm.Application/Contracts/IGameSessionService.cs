namespace Swarm.Application.Contracts;

public interface IGameSessionService
{
    void StartNewSession(StageConfig config);
    void ApplyInput(float dirX, float dirY, float speed);
    void Stop();
    void Fire();
    void Tick(float deltaSeconds);
    GameSnapshot GetSnapshot();
}
