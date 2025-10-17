namespace Swarm.Application.Contracts;

public interface ISaveConfig
{
    string BasePath { get; }
    string EncryptionKey { get; }
}
