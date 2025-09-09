using Swarm.Application.Contracts;

namespace Swarm.Application.Config;

public sealed record class SaveConfig(string BasePath) : ISaveConfig;
