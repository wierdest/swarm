using Swarm.Domain.Common;

namespace Swarm.Domain.Time;

public readonly record struct Cooldown
{
    public readonly float PeriodSeconds;
    public readonly float RemainingSeconds;

    public Cooldown(float periodSeconds, float remainingSeconds)
    {
        Guard.Positive(periodSeconds, nameof(Cooldown));
        Guard.Finite(remainingSeconds, nameof(remainingSeconds));
        Guard.True(remainingSeconds >= 0f, "remainingSeconds must be >= 0");
        PeriodSeconds = periodSeconds;
        RemainingSeconds = remainingSeconds;
    }

    public Cooldown(float periodSeconds) : this(periodSeconds, 0f) { }
    
    public bool IsReady => RemainingSeconds <= 0f;

    public Cooldown Start() => new(PeriodSeconds, PeriodSeconds);

    public Cooldown ConsumeIfReady(out bool consumed)
    {
        if (IsReady) { consumed = true; return Start(); }
        consumed = false; return this;
    }

    public Cooldown Tick(DeltaTime dt)
    {
        var next = RemainingSeconds - dt;
        return next <= 0f ? new Cooldown(PeriodSeconds, 0f) : new Cooldown(PeriodSeconds, next);
    }
}
