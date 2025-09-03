using Swarm.Domain.Common;

namespace Swarm.Domain.Primitives;

public readonly record struct Radius
{
    public readonly float Value;
    public Radius(float value)
    {
        Guard.Positive(value, nameof(Radius));

        Value = value;
    }

    public static implicit operator float(Radius radius) => radius.Value;

    public static explicit operator Radius(float value) => new(value);
}