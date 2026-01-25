using Swarm.Domain.Common;
using Swarm.Domain.Combat;

namespace Swarm.Domain.Primitives;

public readonly record struct ComparisonOperator
{
    public string Value { get; }

    public ComparisonOperator(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Comparison operator is required.");

        Value = value switch
        {
            ">=" or ">" or "<=" or "<" or "==" or "!=" => value,
            _ => throw new DomainException($"Unsupported comparison operator '{value}'.")
        };
    }

    public bool Evaluate(Score current, Score target) => Value switch
    {
        ">=" => current.Value >= target.Value,
        ">" => current.Value > target.Value,
        "<=" => current.Value <= target.Value,
        "<" => current.Value < target.Value,
        "==" => current.Value == target.Value,
        "!=" => current.Value != target.Value,
        _ => throw new DomainException("Invalid comparison operator.")
    };
}
