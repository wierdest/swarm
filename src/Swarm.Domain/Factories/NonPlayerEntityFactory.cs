using Swarm.Domain.Combat;
using Swarm.Domain.Entities.NonPlayerEntities;
using Swarm.Domain.Entities.NonPlayerEntities.Behaviours;
using Swarm.Domain.Entities.NonPlayerEntities.Behaviours.Strategies;
using Swarm.Domain.Entities.NonPlayerEntities.DeathTriggers;
using Swarm.Domain.Entities.Projectiles;
using Swarm.Domain.Entities.Weapons;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;

namespace Swarm.Domain.Factories;

public static class NonPlayerEntityFactory
{
    public static Zombie CreateZombie(
        Vector2 startPosition,
        Radius radius,
        HitPoints hp,
        float speed,
        float? targetThreshold,
        float? dodgeThreshold
    )
    {
        var behaviour = new SeekBehaviour(
            speed: speed,
            targetStrategy: new NearestHealthyOrPlayerTargetStrategy(
                threshold: targetThreshold ?? radius.Value * 10f
            ),
            actionStrategy: null,
            dodgeStrategy: new NearestProjectileDodgeStrategy(
                owner: ProjectileOwnerTypes.Player,
                threshold: dodgeThreshold ?? radius.Value * 10f
            ),
            runawayStrategy: null
        );

        return new Zombie(
            id: EntityId.New(),
            startPosition: startPosition,
            radius: radius,
            hp: hp,
            behaviour: behaviour
        );
    }

    public static Healthy CreateHealthy(
        Vector2 startPosition,
        Radius radius,
        HitPoints hp,
        float speed,
        Vector2 safehouse,
        float? dodgeThreshold,
        float? infectedSpeed,
        float? infectedTargetThreshold,
        float? infectedDodgeThreshold
    )
    {
        var healthyBehaviour = new SeekBehaviour(
            speed: speed,
            targetStrategy: new SafehouseTargetStrategy(
                safehouse: safehouse
            ),
            actionStrategy: null,
            dodgeStrategy: new NearestProjectileDodgeStrategy(
                owner: ProjectileOwnerTypes.All,
                threshold: dodgeThreshold ?? radius.Value * 10f
            ),
            runawayStrategy: null
        );

        INonPlayerEntityBehaviour? infectedBehaviour = null;
        if (infectedSpeed is not null &&
            infectedTargetThreshold is not null &&
            infectedDodgeThreshold is not null)
        {
            infectedBehaviour = new SeekBehaviour(
                speed: infectedSpeed.Value,
                targetStrategy: new NearestHealthyOrPlayerTargetStrategy(
                    threshold: infectedTargetThreshold.Value
                ),
                actionStrategy: null,
                dodgeStrategy: new NearestProjectileDodgeStrategy(
                    owner: ProjectileOwnerTypes.Player,
                    threshold: infectedDodgeThreshold.Value
                ),
                runawayStrategy: null
            );
        }

        return new Healthy(
            id: EntityId.New(),
            startPosition: startPosition,
            radius: radius,
            hp: hp,
            behaviours: [healthyBehaviour, infectedBehaviour]
        );
    }

    public static Shooter CreateShooter(
        Vector2 startPosition,
        Radius radius,
        HitPoints hp,
        float speed,
        float shootRange,
        float? dodgeThreshold,
        int? runawayThreshold,
        Vector2 safehouse,
        float safehouseWeight,
        float avoidPlayerWeight,
        Weapon weapon,
        int minionSpawnCount,
        Radius minionRadius
    )
    {
        var behaviour = new SeekBehaviour(
            speed: speed,
            targetStrategy: new PlayerTargetStrategy(),
            actionStrategy: new RangeShootStrategy(
                shootRange: shootRange
            ),
            dodgeStrategy: new NearestProjectileDodgeStrategy(
                owner: ProjectileOwnerTypes.Player,
                threshold: dodgeThreshold ?? radius.Value * 10f
            ),
            runawayStrategy: new PlayerToSafehouseRunawayStrategy(
                threshold: runawayThreshold ?? hp.Value / 2,
                safehouse: new Vector2(
                    safehouse.X + radius.Value * 2f,
                    safehouse.Y - radius.Value * 2f
                ),
                safehouseWeight: safehouseWeight,
                avoidPlayerWeight: avoidPlayerWeight
            )
        );

        return new Shooter(
            id: EntityId.New(),
            startPosition: startPosition,
            radius: radius,
            hp: hp,
            behaviour: behaviour,
            weapon: weapon,
            deathTrigger: new SpawnZombiesDeathTrigger(
                minionSpawnCount,
                minionRadius
            )
        );
    }
}
