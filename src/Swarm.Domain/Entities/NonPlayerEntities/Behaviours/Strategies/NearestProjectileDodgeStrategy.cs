using Swarm.Domain.Entities.Projectiles;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities.Behaviours.Strategies;

public sealed class NearestProjectileDodgeStrategy(
    ProjectileOwnerTypes owner,
    float threshold,
    float multiplier = 1.5f
) : IDodgeStrategy
{
    private readonly float _thresholdSq = threshold * threshold;
    public Direction? DecideDodge(NonPlayerEntityContext<INonPlayerEntity> context)
    {
        var nearest = context.FindNearestProjectile(owner, threshold);
        if (nearest is null) return null;
        
        var delta = nearest.Position - context.Position;

        var dodgeVector = new Vector2(-delta.Y, delta.X);

        // perpendicular to projectile direction
        float distSq = delta.LengthSquared();
        float distanceFactor = 1f - (distSq / _thresholdSq);
        float dodgeStrength = 1f + distanceFactor * (multiplier - 1f);

        dodgeVector *= dodgeStrength;

        return Direction.From(dodgeVector.X, dodgeVector.Y);
    }
}
