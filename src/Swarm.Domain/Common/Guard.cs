namespace Swarm.Domain.Common;

public static class Guard
{
    public static void True(bool condition, string message) =>
        _ = condition ? true : throw new DomainException(message);
     public static void Positive(float value, string name)
        => True(value > 0f && float.IsFinite(value), $"{name} must be > 0 and finite.");
    public static void NonNegative(int value, string name)
        => True(value >= 0, $"{name} must be non-negative.");
    public static void Finite(float value, string name)
        => True(float.IsFinite(value), $"{name} must be finite.");
}
