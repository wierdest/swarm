namespace Swarm.Domain.Abstractions;

public sealed class DomainException(string message) : Exception(message);
