using Swarm.Domain.Common;

namespace Swarm.Domain.Time;

public readonly record struct RoundTimer
{
    public readonly int Seconds;

    public RoundTimer(int seconds) : this()
    {
        Guard.NonNegative(seconds, nameof(seconds));
        Guard.MaxSeconds(seconds, 999, nameof(seconds));
        Seconds = seconds;
    }

    public bool IsExpired => Seconds <= 0;

    public static RoundTimer Reset(int seconds) => new(seconds);

    public RoundTimer Tick(int delta = 1)
    {
        var newValue = Seconds - delta;
        return new RoundTimer(Math.Max(newValue, 0));
    }

    public RoundTimer Add(int extraSeconds)
    {
        var newValue = Seconds + extraSeconds;
        return new RoundTimer(Math.Min(newValue, 999));
    }

    public override string ToString() => $"{Seconds:000}s";
    
}
