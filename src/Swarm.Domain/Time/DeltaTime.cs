using Swarm.Domain.Common;

namespace Swarm.Domain.Time;

public readonly record struct DeltaTime
{
    public readonly float Seconds;

    public DeltaTime(float seconds)
    {
        Guard.Positive(seconds, nameof(DeltaTime));
        Seconds = seconds;
    }

    public static implicit operator float(DeltaTime deltaTime) => deltaTime.Seconds; 
}