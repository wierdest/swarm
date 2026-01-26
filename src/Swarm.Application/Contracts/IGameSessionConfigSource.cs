namespace Swarm.Application.Contracts;

public interface IGameSessionConfigSource
{
    string LoadConfigJson(string contentRootDirectory);
    GameSessionConfigManifest LoadManifest(string contentRootDirectory);
    int SelectEntryIndex(GameSessionConfigManifest manifest);
    void SaveManifest(string contentRootDirectory, GameSessionConfigManifest manifest);
}

public sealed class GameSessionConfigManifest
{
    public int? ActiveIndex { get; set; }
    public List<GameSessionConfigEntry> Entries { get; set; } = new();
}

public sealed class GameSessionConfigEntry
{
    public string? Name { get; set; }
    public string File { get; set; } = string.Empty;
    public bool Completed { get; set; }
}
