using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Swarm.Application;
using Swarm.Application.Contracts;
using Swarm.Application.Primitives;

namespace Swarm.Infrastructure.Persistence;

public sealed class EncryptedJsonSaveGameRepository : ISaveGameRepository
{
    private readonly string _basePath;
    private readonly byte[] _key;
    private readonly byte[] _iv;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
    };

    public EncryptedJsonSaveGameRepository(ISaveConfig settings)
    {
        _basePath = settings.BasePath ?? throw new ArgumentNullException(nameof(settings));
        Directory.CreateDirectory(_basePath);

        if (string.IsNullOrWhiteSpace(settings.EncryptionKey))
            throw new ArgumentNullException(nameof(settings.EncryptionKey), "Encryption key is required for save encryption.");

        // Derive fixed-length key/IV from provided key string
        using var sha256 = SHA256.Create();
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(settings.EncryptionKey));
        _iv = _key.Take(16).ToArray(); // AES needs 16-byte IV
    }

    public async Task SaveAsync(SaveGame save, SaveName saveName, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_basePath, $"{saveName.Value}.sav");

        List<SaveGame> saves = new();
        if (File.Exists(filePath))
        {
            var encrypted = await File.ReadAllBytesAsync(filePath, cancellationToken);
            var json = DecryptString(encrypted);
            var existing = JsonSerializer.Deserialize<List<SaveGame>>(json, _jsonOptions);
            if (existing is not null)
                saves = existing;
        }

        saves.Insert(0, save);

        var updatedJson = JsonSerializer.Serialize(saves, _jsonOptions);
        var encryptedData = EncryptString(updatedJson);

        await File.WriteAllBytesAsync(filePath, encryptedData, cancellationToken);
    }

    public async Task<SaveGame?> LoadLatestAsync(SaveName saveName, CancellationToken cancellationToken = default)
    {
        var all = await LoadAllAsync(saveName, cancellationToken);
        return all.Count > 0 ? all[0] : null; // latest is first
    }

    public async Task<IReadOnlyList<SaveGame>> LoadAllAsync(SaveName saveName, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_basePath, $"{saveName.Value}.sav");
        if (!File.Exists(filePath))
            return [];

        var encrypted = await File.ReadAllBytesAsync(filePath, cancellationToken);
        var json = DecryptString(encrypted);

        var saves = JsonSerializer.Deserialize<List<SaveGame>>(json, _jsonOptions);

        return saves?.ToArray() ?? [];
    }


    private byte[] EncryptString(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return ms.ToArray();
    }

    private string DecryptString(byte[] cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(cipherText);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}
