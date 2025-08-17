
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
}
