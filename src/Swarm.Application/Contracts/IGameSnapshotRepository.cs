using Swarm.Application.Primitives;

namespace Swarm.Application.Contracts;

public interface IGameSnapshotRepository
{
    Task SaveAsync(GameSnapshot snapshot, SaveName saveName, CancellationToken cancellationToken = default);
    Task<GameSnapshot?> LoadAsync(SaveName saveName, CancellationToken cancellationToken = default);

}
