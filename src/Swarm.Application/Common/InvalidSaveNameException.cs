namespace Swarm.Application.Common;

public sealed class InvalidSaveNameException(string message) : Exception(message);
