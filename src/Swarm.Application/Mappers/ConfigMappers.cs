using Swarm.Application.Config;
using Swarm.Domain.Common;
using Swarm.Domain.Combat;
using Swarm.Domain.Entities;
using Swarm.Domain.Entities.Projectiles;
using Swarm.Domain.Entities.Weapons;
using Swarm.Domain.Entities.Weapons.Patterns;
using Swarm.Domain.Factories;
using Swarm.Domain.Factories.Strategies;
using Swarm.Domain.GameObjects;
using Swarm.Domain.GameObjects.Spawners;
using Swarm.Domain.GameObjects.Spawners.Behaviours;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;
using Swarm.Domain.Interfaces;

namespace Swarm.Application.Mappers;

static class ConfigMappers
{
    public static Bounds ToStage(GameSessionConfig config)
    {
        var stageConfig = config.StageConfig;

        return new Bounds(
            stageConfig.Left,
            stageConfig.Top,
            stageConfig.Right,
            stageConfig.Bottom
        );
    }

    public static Player ToPlayer(GameSessionConfig config, Bounds stage)
    {
        var level = config.LevelConfig;

        var playerStart = level.PlayerAreaConfig is AreaConfig playerArea
            ? new Vector2(playerArea.X, playerArea.Y)
            : stage.Center;
        var playerRadius = new Radius(config.PlayerRadius);
        var player = new Player(
            EntityId.New(),
            playerStart,
            playerRadius
        );

        if (level.Weapon is WeaponConfig weaponConfig)
        {
            var pattern = new SingleShotPattern(
                new Damage(weaponConfig.Damage),
                weaponConfig.ProjectileSpeed,
                new Radius(weaponConfig.ProjectileRadius),
                weaponConfig.ProjectileLifetimeSeconds
            );

            var cooldown = new Cooldown(1f / weaponConfig.RatePerSecond);

            var playerWeapon = new PlayerWeapon(
                weaponConfig.Name,
                pattern,
                cooldown,
                ProjectileOwnerTypes.Player,
                weaponConfig.MaxAmmo,
                WeaponTypes.Automatic
            );

            player.SetWeapon(playerWeapon);
        }

        return player;
    }

    public static List<Wall> ToWalls(LevelConfig level, Bounds stage)
    {
        var walls = new List<Wall>();

        if (level.Walls is { Count: > 0 } wallConfigs)
        {
            walls = [.. WallFactory.CreateWallsFromList(
                wallConfigs.Select(wall => (new Vector2(wall.X, wall.Y), new Radius(wall.Radius)))
            )];
        }
        else if (level.WallGeneratorConfig is WallGeneratorConfig wallGeneratorConfig)
        {
            walls = [.. WallFactory.CreateVoronoiWalls(
                start: stage.TopLeftCorner,
                end: stage.BottomRightCorner,
                levelBounds: stage,
                wallRadius: wallGeneratorConfig.WallRadius,
                seedCount: wallGeneratorConfig.WallSeedCount,
                wallDensity: wallGeneratorConfig.WallDensity,
                cellSize: wallGeneratorConfig.WallCellSize,
                minWallCount: GetTotalSpawnerCount(level),
                seed: wallGeneratorConfig.WallRandomSeed
            )];
        }

        if (walls.Count > 0)
        {
            var spawnerPlacementStrategy = new OpenSideSpawnerStrategy(walls);
            foreach (var wall in walls)
            {
                wall.Spawners = spawnerPlacementStrategy.GetSpawnerPositions(wall, stage);
            }
        }

        return walls;
    }

    public static List<Bomb> ToBombs(LevelConfig level)
    {
        var bombs = new List<Bomb>();

        if (level.Bombs is { Count: > 0 } bombConfigs)
        {
            foreach (var bombConfig in bombConfigs)
            {
                bombs.Add(new Bomb(
                    bombConfig.Identifier,
                    new Cooldown(bombConfig.CooldownSeconds)
                ));
            }
        }

        return bombs;
    }

    public static RoundTimer ToTimer(GameSessionConfig config) => new(config.RoundLength);

    public static IGoal ToGoal(LevelConfig level)
    {
        if (level.GoalConfig is not GoalConfig goalConfig)
            throw new DomainException("Goal config is required to start a session.");

        return GoalFactory.Create(
            goalConfig.GoalDescription,
            goalConfig.GameSessionPropertyIdentifier,
            goalConfig.Operator,
            goalConfig.TargetValue
        );
    }

    public static PlayerArea? ToPlayerArea(GameSession session, LevelConfig level)
    {
        if (level.PlayerAreaConfig is AreaConfig playerAreaConfig)
        {
            return new PlayerArea(
                session,
                new Vector2(playerAreaConfig.X, playerAreaConfig.Y),
                new Radius(playerAreaConfig.Radius)
            );
        }

        return null;
    }

    public static TargetArea? ToTargetArea(GameSession session, LevelConfig level)
    {
        if (level.TargetAreaConfig is AreaConfig targetAreaConfig)
        {
            return new TargetArea(
                session,
                new Vector2(targetAreaConfig.X, targetAreaConfig.Y),
                new Radius(targetAreaConfig.Radius)
            );
        }

        return null;
    }

    public static List<NonPlayerEntitySpawner> ToSpawners(
        GameSession session,
        LevelConfig level,
        PlayerArea? playerArea,
        TargetArea? targetArea,
        Bounds stage)
    {
        var spawners = new List<NonPlayerEntitySpawner>();
        var spawnerPositions = session.SpawnerPoints.ToList();
        var random = new Random();

        if (level.ZombieConfig is NonPlayerEntityConfig zombieConfig)
        {
            if (zombieConfig.TargetConfig is not TargetConfig targetConfig ||
                zombieConfig.DodgeConfig is not DodgeConfig dodgeConfig)
                throw new DomainException("Zombie config must have TargetConfig defined.");

            var zombieSpawners = zombieConfig.Spawners;

            if (zombieSpawners is not null && zombieSpawners.Count > 0)
            {
                foreach (var spawnerConfig in zombieSpawners)
                {
                    if (spawnerPositions.Count == 0) throw new DomainException("No available spawner positions left!");

                    var index = random.Next(spawnerPositions.Count);
                    var spawnPos = spawnerPositions[index];
                    spawnerPositions.RemoveAt(index);

                    var spawner = new NonPlayerEntitySpawner(
                        session,
                        new FixedPositionNonPlayerEntitySpawnerBehaviour(
                            position: spawnPos,
                            cooldownSeconds: spawnerConfig.CooldownSeconds,
                            entityFactory: pos =>
                            {
                                return NonPlayerEntityFactory.CreateZombie(
                                    startPosition: pos,
                                    radius: new(zombieConfig.Radius),
                                    hp: new(zombieConfig.HP),
                                    speed: zombieConfig.Speed,
                                    targetThreshold: targetConfig.Threshold,
                                    dodgeThreshold: dodgeConfig.Threshold,
                                    dodgeMultiplier: dodgeConfig.SpeedMultiplier
                                );
                            }
                        ),
                        spawnerConfig.BatchSize > 0 ? spawnerConfig.BatchSize : 1
                    );

                    spawners.Add(spawner);
                }
            }
        }

        if (level.HealthyConfig is HealthyConfig healthyConfig)
        {
            var entityConfig = healthyConfig.NonPlayerEntityConfig;
            if (entityConfig.TargetConfig is not TargetConfig targetConfig ||
                entityConfig.DodgeConfig is not DodgeConfig dodgeConfig)
                throw new DomainException("Healthy config must have TargetConfig defined.");

            var healthySpawners = entityConfig.Spawners;

            if (healthySpawners is null || healthySpawners.Count == 0)
                throw new DomainException("Healthy config provided but no healthy spawner defined in non player entity config.");

            foreach (var spawnerConfig in healthySpawners)
            {
                if (spawnerPositions.Count == 0) throw new DomainException("No available spawner positions left!");

                var index = random.Next(spawnerPositions.Count);
                var spawnPos = spawnerPositions[index];
                spawnerPositions.RemoveAt(index);

                var batch = spawnerConfig.BatchSize > 0 ? spawnerConfig.BatchSize : 1;
                var spawner = new NonPlayerEntitySpawner(
                    session,
                    new FixedPositionNonPlayerEntitySpawnerBehaviour(
                        position: spawnPos,
                        cooldownSeconds: spawnerConfig.CooldownSeconds,
                        entityFactory: pos => NonPlayerEntityFactory.CreateHealthy(
                            startPosition: pos,
                            radius: new(entityConfig.Radius),
                            hp: new(entityConfig.HP),
                            speed: entityConfig.Speed,
                            safehouse: playerArea is null ? stage.LeftRandomCorner : playerArea.Position,
                            dodgeThreshold: dodgeConfig.Threshold ?? 150f,
                            infectedSpeed: healthyConfig.InfectedSpeed,
                            infectedTargetThreshold: healthyConfig.InfectedTargetThreshold,
                            infectedDodgeThreshold: healthyConfig.InfectedDodgeThreshold
                        )
                    ),
                    batch
                );

                spawners.Add(spawner);
            }
        }

        if (level.ShooterConfig is ShooterConfig shooterConfig)
        {
            var entityConfig = shooterConfig.NonPlayerEntityConfig;

            if (entityConfig.TargetConfig is not TargetConfig targetConfig ||
                entityConfig.DodgeConfig is not DodgeConfig dodgeConfig ||
                shooterConfig.RunawayConfig is not RunawayConfig runawayConfig)
                throw new DomainException("Shooter config must have TargetConfig, DodgeConfig and RunawayConfig defined.");

            var shooterSpawners = entityConfig.Spawners;

            if (shooterSpawners is null || shooterSpawners.Count == 0)
                throw new DomainException("Shooter config provided but no shooter spawner defined in level config.");

            if (shooterConfig.Weapon is null)
                throw new DomainException("Shooter config must have a weapon defined.");

            var spawnConfig = shooterConfig.ZombieSpawnedOnDeathTriggerConfig;
            var minionTargetConfig = spawnConfig?.TargetConfig;
            var minionDodgeConfig = spawnConfig?.DodgeConfig;

            var bossWeapon = shooterConfig.Weapon;

            var pattern = new SingleShotPattern(
                new Damage(bossWeapon.Damage),
                bossWeapon.ProjectileSpeed,
                new Radius(bossWeapon.ProjectileRadius),
                bossWeapon.ProjectileLifetimeSeconds
            );

            var cooldown = new Cooldown(1f / bossWeapon.RatePerSecond);

            var weapon = new Weapon(pattern, cooldown, ProjectileOwnerTypes.Enemy);

            var safehouse = targetArea is not null
                ? targetArea.Position
                : stage.RightRandomCorner;

            foreach (var spawnerConfig in shooterSpawners)
            {
                if (spawnerPositions.Count == 0) throw new DomainException("No available spawner positions left!");

                var index = random.Next(spawnerPositions.Count);
                var spawnPos = spawnerPositions[index];
                spawnerPositions.RemoveAt(index);

                var batch = spawnerConfig.BatchSize > 0 ? spawnerConfig.BatchSize : 1;
                var spawner = new NonPlayerEntitySpawner(
                    session,
                    new FixedPositionNonPlayerEntitySpawnerBehaviour(
                        position: spawnPos,
                        cooldownSeconds: spawnerConfig.CooldownSeconds,
                        entityFactory: pos => NonPlayerEntityFactory.CreateShooter(
                            startPosition: pos,
                            radius: new(entityConfig.Radius),
                            hp: new(entityConfig.HP),
                            speed: entityConfig.Speed,
                            shootRange: shooterConfig.ShootRange ?? 600f,
                            dodgeThreshold: dodgeConfig.Threshold ?? 150f,
                            runawayThreshold: runawayConfig.Threshold ?? 9,
                            safehouse: safehouse,
                            safehouseWeight: runawayConfig.SafehouseWeight ?? 0.5f,
                            avoidPlayerWeight: runawayConfig.AvoidPlayerWeight ?? 0.5f,
                            weapon: weapon,
                            minionSpawnCount: shooterConfig.MinionSpawnCount,
                            minionRadius: spawnConfig is null ? null : new Radius(spawnConfig.Radius),
                            minionHp: spawnConfig is null ? null : new HitPoints(spawnConfig.HP),
                            minionSpeed: spawnConfig?.Speed,
                            minionTargetThreshold: minionTargetConfig?.Threshold,
                            minionDodgeThreshold: minionDodgeConfig?.Threshold,
                            minionDodgeMultiplier: minionDodgeConfig?.SpeedMultiplier
                            )
                        ),
                    batch
                );

                spawners.Add(spawner);
            }
        }

        return spawners;
    }

    private static int GetTotalSpawnerCount(LevelConfig level)
    {
        var total = 0;

        if (level.ZombieConfig?.Spawners is { Count: > 0 } zombieSpawners)
        {
            total += zombieSpawners.Count;
        }

        if (level.HealthyConfig?.NonPlayerEntityConfig.Spawners is { Count: > 0 } healthySpawners)
        {
            total += healthySpawners.Count;
        }

        if (level.ShooterConfig?.NonPlayerEntityConfig.Spawners is { Count: > 0 } shooterSpawners)
        {
            total += shooterSpawners.Count;
        }

        return total;
    }
}
