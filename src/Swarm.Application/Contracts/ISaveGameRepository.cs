using Swarm.Application.Primitives;

namespace Swarm.Application.Contracts;

public interface ISaveGameRepository
{
    Task SaveAsync(SaveGame save, SaveName saveName, CancellationToken cancellationToken = default);
    Task<SaveGame?> LoadLatestAsync(SaveName saveName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SaveGame>> LoadAllAsync(SaveName saveName, CancellationToken cancellationToken = default);
}
