using Swarm.Domain.Common;

namespace Swarm.Domain.Combat;
// todo move this to primitives, it actually is an int scalar
public readonly record struct Score
{
    public int Value { get; }

    public Score(int value)
    {
        Guard.NonNegative(value, nameof(Score));
        Value = value;
    }

    public Score Add(int amount)
    {
        Guard.NonNegative(amount, nameof(amount));
        checked { return new Score(Value + amount); }
    }

    public static Score operator +(Score score, int amount) => score.Add(amount);

    public override string ToString() => Value.ToString();

    public static explicit operator Score(int value) => new(value);
    
    public static implicit operator int(Score d) => d.Value;
}