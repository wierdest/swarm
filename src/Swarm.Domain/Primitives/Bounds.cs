
using Swarm.Domain.Common;

namespace Swarm.Domain.Primitives;

public readonly record struct Bounds
{
    public readonly float Left;
    public readonly float Top;
    public readonly float Right;
    public readonly float Bottom;

    public Bounds(float left, float top, float right, float bottom)
    {
        Guard.True(right > left, "Bounds.Right must be > Left.");
        Guard.True(bottom > top, "Bounds.Bottom must be > Top.");

        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public float Width => Right - Left;
    public float Height => Bottom - Top;
    public Vector2 Center => new(
        (Left + Right) * 0.5f,
        (Top + Bottom) * 0.5f);
    public Vector2 LeftRandomCorner => Random.Shared.Next(2) == 0
        ? new(Left, Top)
        : new(Left, Bottom);
    public Vector2 RightRandomCorner => Random.Shared.Next(2) == 0
        ? new(Right, Top)
        : new(Right, Bottom);

    public Vector2 TopLeftCorner => new(Left, Top);
    public Vector2 TopRightCorner => new(Right, Top);
    public Vector2 BottomLeftCorner => new(Left, Bottom);
    public Vector2 BottomRightCorner => new(Right, Bottom);

    public Vector2 Clamp(Vector2 p) => new(
       MathF.Min(MathF.Max(p.X, Left), Right),
       MathF.Min(MathF.Max(p.Y, Top), Bottom));
    
    public bool Contains(Vector2 p) =>
        p.X >= Left && p.X <= Right &&
        p.Y >= Top && p.Y <= Bottom;
    
}
