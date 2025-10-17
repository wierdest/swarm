using Swarm.Domain.Interfaces;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities.NonPlayerEntities.Behaviours.Strategies;

public sealed class RangeShootStrategy(
    float shootRange
) : IActionStrategy
{
    public bool ShouldAct(NonPlayerEntityContext context, ref Cooldown cooldown)
    {
        cooldown = cooldown.Tick(context.DeltaTime);
        cooldown = cooldown.ConsumeIfReady(out var ready);

        if (!ready)
            return false;

        var delta = context.PlayerPosition - context.Position;
        return delta.LengthSquared() <= shootRange * shootRange;
    }

}
