using Swarm.Domain.Entities.Projectiles;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.NonPlayerEntities.Behaviours.Strategies;

public sealed class NearestProjectileDodgeStrategy(
    float dodgeDistanceThreshold,
    float maxDodgeMultiplier = 1.5f
    ) : IDodgeStrategy
{
    public Direction? DecideDodge(NonPlayerEntityContext context)
    {
        if (context.Projectiles.Count == 0)
        {
            return null;
        }

        float thresholdSq = dodgeDistanceThreshold * dodgeDistanceThreshold;

        Vector2? nearestProjectile = null;
        float nearestDistSq = float.MaxValue;


        foreach (var proj in context.Projectiles)
        {
            if (proj.Owner != ProjectileOwnerTypes.Player)
                continue;

            var delta = proj.Position - context.Position;
            float distSq = delta.LengthSquared();

            if (distSq < nearestDistSq && distSq < thresholdSq)
            {
                nearestDistSq = distSq;
                nearestProjectile = proj.Position;
            }
        }

        if (nearestProjectile is null)
        {
            return null;
        }

        // perpendicular to projectile direction
        var toProjectile = nearestProjectile.Value - context.Position;
        var dodgeVector = new Vector2(-toProjectile.Y, toProjectile.X);

        float distanceFactor = 1f - (nearestDistSq / thresholdSq);  
        float dodgeStrength = 1f + distanceFactor * (maxDodgeMultiplier - 1f);

        dodgeVector *= dodgeStrength;

        return Direction.From(dodgeVector.X, dodgeVector.Y);
    }
}
