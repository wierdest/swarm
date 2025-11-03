using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities.NonPlayerEntities.Behaviours;

public abstract class NonPlayerEntityBehaviourBase(
    float speed,
    IActionStrategy? actionStrategy,
    IDodgeStrategy dodgeStrategy,
    IRunawayStrategy? runawayStrategy,
    Cooldown? initialCooldown = null
) : INonPlayerEntityBehaviour
{
    protected readonly float Speed = speed;
    protected readonly IActionStrategy? ActionStrategy = actionStrategy;
    protected readonly IDodgeStrategy DodgeStrategy = dodgeStrategy;
    protected readonly IRunawayStrategy? RunawayStrategy = runawayStrategy;
    protected Cooldown Cooldown = initialCooldown ?? Cooldown.AlwaysReady;

    public virtual bool DecideAction(NonPlayerEntityContext context)
    {
        if (ActionStrategy is null) return false;
        return ActionStrategy.ShouldAct(context, ref Cooldown);
    }

    public (Direction direction, float speed)? DecideMovement(NonPlayerEntityContext context)
    {
        var dodge = DodgeStrategy.DecideDodge(context);
        if (dodge is not null)
            return (dodge.Value, Speed);

        if (RunawayStrategy is not null)
        {
            var runaway = RunawayStrategy.DecideRunaway(context);
            if (runaway is not null)
                return (runaway.Value, Speed * RunawayMultiplier());
        }

        return DecidePrimaryMovement(context);
    }

    protected virtual float RunawayMultiplier() => 1.0f;

    protected abstract (Direction direction, float speed)? DecidePrimaryMovement(NonPlayerEntityContext context);
}
