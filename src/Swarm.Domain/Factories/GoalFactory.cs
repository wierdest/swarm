using System.Linq.Expressions;
using Swarm.Domain.Combat;
using Swarm.Domain.Common;
using Swarm.Domain.Entities;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Factories;

public static class GoalFactory
{
    public static IGoal Create(
        string description,
        string gameSessionPropertyIdentifier,
        string comparisonOperator,
        int targetValue
    )
    {
        if (string.IsNullOrWhiteSpace(gameSessionPropertyIdentifier))
            throw new DomainException("Game session property identifier is required.");

        var target = new Score(targetValue);
        var comparison = new ComparisonOperator(comparisonOperator);
        var getter = BuildGetter(gameSessionPropertyIdentifier);

        return new ReflectionGoal(description, target, getter, comparison);
    }

    private static Func<GameSession, Score> BuildGetter(string propertyName)
    {
        var sessionParam = Expression.Parameter(typeof(GameSession), "session");
        Expression memberAccess;

        try
        {
            memberAccess = Expression.PropertyOrField(sessionParam, propertyName);
        }
        catch (ArgumentException)
        {
            throw new DomainException($"GameSession does not contain '{propertyName}'.");
        }

        Expression body = memberAccess.Type == typeof(Score)
            ? memberAccess
            : memberAccess.Type == typeof(RoundTimer)
                ? Expression.New(
                    typeof(Score).GetConstructor([typeof(int)])!,
                    Expression.PropertyOrField(memberAccess, nameof(RoundTimer.Seconds)))
                : throw new DomainException($"Unsupported property type '{memberAccess.Type.Name}' for '{propertyName}'.");

        return Expression.Lambda<Func<GameSession, Score>>(body, sessionParam).Compile();
    }

}

internal sealed class ReflectionGoal(string description, Score target, Func<GameSession, Score> getter, ComparisonOperator comparison) : IGoal
{
    private readonly Func<GameSession, Score> _getter = getter;
    private readonly ComparisonOperator _comparison = comparison;

    public Score Target { get; } = target;
    public string Description { get; } = description;

    public bool Evaluate(GameSession session) => _comparison.Evaluate(_getter(session), Target);
}
