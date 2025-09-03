using Swarm.Domain.Common;

namespace Swarm.Domain.Primitives;

public readonly record struct Direction
{
    public readonly Vector2 Vector;

    public float X => Vector.X;
    public float Y => Vector.Y;
    public Direction(Vector2 vector)
    {
        var lenSq = vector.LengthSquared();
        Guard.True(lenSq > 0.999f && lenSq < 1.001f, "Direction must be normalized.");

        Vector = vector;
    }

    public static Direction From(float x, float y) => new(new Vector2(x, y).Normalized());
    public static Direction From(Vector2 vector2) => new(vector2.Normalized());

    public Direction Rotated(float radians)
    {
        var cos = MathF.Cos(radians);
        var sin = MathF.Sin(radians);
        var x = Vector.X * cos - Vector.Y * sin;
        var y = Vector.X * sin + Vector.Y * cos;
        return new Direction(new Vector2(x, y));
    }

}