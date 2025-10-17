using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities.NonPlayerEntities.Behaviours;

public sealed class ChaseBehaviour(
    float speed,
    IActionStrategy? actionStrategy,
    IDodgeStrategy dodgeStrategy,
    IRunawayStrategy? runawayStrategy
    ) : INonPlayerEntityBehaviour
{
    private Cooldown _cooldown = Cooldown.AlwaysReady;
    public bool DecideAction(NonPlayerEntityContext context)
    {
        if (actionStrategy is null) return false;

        return actionStrategy.ShouldAct(context, ref _cooldown);

    }
    public (Direction direction, float speed)? DecideMovement(NonPlayerEntityContext context)
    {
        var dodgeDir = dodgeStrategy.DecideDodge(context);

        if (dodgeDir is not null)
            return (dodgeDir.Value, speed);

        if (runawayStrategy is not null)
        {
            var runawayDir = runawayStrategy.DecideRunaway(context);
            if (runawayDir is not null)
                return (runawayDir.Value, speed * 2.0f);
        }
        
        var toPlayer = context.PlayerPosition - context.Position;

        if (toPlayer.IsZero())
            return null;

        var direction = Direction.From(toPlayer.X, toPlayer.Y);

        return (direction, speed);
    }
}
