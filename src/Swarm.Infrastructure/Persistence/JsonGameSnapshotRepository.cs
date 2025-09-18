using System.Text.Json;
using Swarm.Application.Contracts;
using Swarm.Application.Primitives;


namespace Swarm.Infrastructure.Persistence;

public class JsonGameSnapshotRepository : IGameSnapshotRepository
{
    private readonly string _basePath;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public JsonGameSnapshotRepository(ISaveConfig settings)
    {
        _basePath = settings.BasePath ?? throw new ArgumentNullException(nameof(settings));
        // idempotent
        Directory.CreateDirectory(_basePath);
    }

    public async Task SaveAsync(GameSnapshot snapshot, SaveName saveName, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_basePath, $"{saveName}.json");
        var json = JsonSerializer.Serialize(snapshot, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    public async Task<GameSnapshot?> LoadAsync(SaveName saveName, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_basePath, $"{saveName}.json");
        if (!File.Exists(filePath)) return null;
        var json = await File.ReadAllTextAsync(filePath, cancellationToken);

        return JsonSerializer.Deserialize<GameSnapshot>(json, _jsonOptions);
    }
}
