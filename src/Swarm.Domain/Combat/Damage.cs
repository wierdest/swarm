using Swarm.Domain.Common;

namespace Swarm.Domain.Combat;

public readonly record struct Damage
{
    public readonly int Value;

    public Damage(int value)
    {
        Guard.NonNegative(value, nameof(Damage));
        Value = value;
    }
}
