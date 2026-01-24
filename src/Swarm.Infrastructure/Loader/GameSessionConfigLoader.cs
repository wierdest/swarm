using System.Text.Json;
using Swarm.Application.Config;
using Swarm.Application.Contracts;

namespace Swarm.Infrastructure.Loader;

public sealed class GameSessionConfigLoader : IGameSessionConfigLoader
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public GameSessionConfig Load(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("Config json is required.", nameof(json));

        try
        {
            var config = JsonSerializer.Deserialize<GameSessionConfig>(json, _options);
            return config ?? throw new InvalidOperationException("GameSessionConfig deserialized to null.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to deserialize GameSessionConfig.", ex);
        }
    }
}
