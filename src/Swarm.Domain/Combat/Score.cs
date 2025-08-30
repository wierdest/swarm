using Swarm.Domain.Common;

namespace Swarm.Domain.Combat;

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
}