using System;
using System.IO;
using System.Text.Json;
using Swarm.Application.Contracts;

namespace Swarm.Infrastructure.Loader;

public sealed class GameSessionConfigManifestSource : IGameSessionConfigSource
{
    private const string DefaultConfigFile = "GameSessionConfig.json";
    private const string ManifestFile = "GameSessionConfigManifest.json";
    private static readonly JsonSerializerOptions _manifestOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };
    private static readonly JsonSerializerOptions _writeOptions = new()
    {
        WriteIndented = true
    };

    public string LoadConfigJson(string contentRootDirectory)
    {
        var manifest = LoadManifest(contentRootDirectory);
        var entry = SelectEntry(manifest);
        if (string.IsNullOrWhiteSpace(entry.File))
            throw new InvalidOperationException("Manifest entry must include a file path.");

        var contentRoot = Path.Combine(AppContext.BaseDirectory, contentRootDirectory);
        var configPath = Path.Combine(contentRoot, entry.File);
        return LoadConfigJsonFromPath(configPath);
    }

    public GameSessionConfigManifest LoadManifest(string contentRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(contentRootDirectory))
            throw new ArgumentException("Content root directory is required.", nameof(contentRootDirectory));

        var contentRoot = Path.Combine(AppContext.BaseDirectory, contentRootDirectory);
        var manifestPath = Path.Combine(contentRoot, ManifestFile);

        if (!File.Exists(manifestPath))
        {
            var fallbackPath = Path.Combine(contentRoot, DefaultConfigFile);
            return new GameSessionConfigManifest
            {
                Entries =
                {
                    new GameSessionConfigEntry
                    {
                        Name = Path.GetFileNameWithoutExtension(fallbackPath),
                        File = DefaultConfigFile,
                        Completed = false
                    }
                }
            };
        }

        try
        {
            var manifestJson = File.ReadAllText(manifestPath);
            var manifest = JsonSerializer.Deserialize<GameSessionConfigManifest>(manifestJson, _manifestOptions);
            if (manifest is null || manifest.Entries.Count == 0)
                throw new InvalidOperationException("Manifest must contain at least one entry.");
            return manifest;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to deserialize GameSessionConfigManifest.json.", ex);
        }
    }

    public void SaveManifest(string contentRootDirectory, GameSessionConfigManifest manifest)
    {
        if (string.IsNullOrWhiteSpace(contentRootDirectory))
            throw new ArgumentException("Content root directory is required.", nameof(contentRootDirectory));
        
        ArgumentNullException.ThrowIfNull(manifest);

        var contentRoot = Path.Combine(AppContext.BaseDirectory, contentRootDirectory);
        var manifestPath = Path.Combine(contentRoot, ManifestFile);
        var json = JsonSerializer.Serialize(manifest, _writeOptions);
        File.WriteAllText(manifestPath, json);
    }

    private static GameSessionConfigEntry SelectEntry(GameSessionConfigManifest manifest)
    {
        if (manifest.ActiveIndex is int idx && idx >= 0 && idx < manifest.Entries.Count)
            return manifest.Entries[idx];

        for (int i = 0; i < manifest.Entries.Count; i++)
        {
            if (!manifest.Entries[i].Completed)
                return manifest.Entries[i];
        }

        return manifest.Entries[0];
    }

    private static string LoadConfigJsonFromPath(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Game session config file not found.", path);

        return File.ReadAllText(path);
    }
}
