using Swarm.Domain.Common;

namespace Swarm.Domain.Combat;

public readonly record struct HitPoints
{
    public readonly int Value;

    public HitPoints(int value)
    {
        Guard.NonNegative(value, nameof(HitPoints));
        Value = value;
    }

    public bool IsZero => Value == 0;

    public HitPoints Heal(int amount)
    {
        Guard.NonNegative(amount, nameof(amount));
        checked { return new HitPoints(Value + amount); }
    }

    public HitPoints Take(int amount)
    {
        Guard.NonNegative(amount, nameof(amount));
        var next = Value - amount;
        return new HitPoints(next < 0 ? 0 : next);
    }
}
