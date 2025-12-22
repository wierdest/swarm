using Swarm.Domain.Common;

namespace Swarm.Domain.Primitives;

public readonly record struct Vector2(float X, float Y)
{
    public static readonly Vector2 Zero = new(0f, 0f);
    public static readonly Vector2 UnitX = new(1f, 0f);
    public static readonly Vector2 UnitY = new(0f, 1f);
    public bool IsZero() => Math.Abs(X) < 1e-6f && Math.Abs(Y) < 1e-6f;
    public float Length() => MathF.Sqrt(X * X + Y * Y);
    public float LengthSquared() => X * X + Y * Y;

    public Vector2 Normalized()
    {
        var len = Length();
        Guard.True(len > 0f, "Cannot normalize vector");
        var inv = 1f / len;
        return new Vector2(X * inv, Y * inv);
    }

    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return new Vector2(
            a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t
        );
    }

    

    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator *(Vector2 v, float s) => new(v.X * s, v.Y * s);
    public static Vector2 operator /(Vector2 v, float s)
    {
        const float Epsilon = 1e-6f;
        Guard.True(MathF.Abs(s) > Epsilon && float.IsFinite(s), "Division by zero or non-finite scalar.");
        return new(v.X / s, v.Y / s);
    }

    public static float Dot(in Vector2 a, in Vector2 b) => a.X * b.X + a.Y * b.Y;

    public static float DistanceSquared(in Vector2 a, in Vector2 b) => (a - b).LengthSquared();

    public static float Distance(in Vector2 a, in Vector2 b) => (a - b).Length();
}
