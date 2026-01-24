using Microsoft.Extensions.Logging;
using Swarm.Application.Contracts;
using Swarm.Application.Config;
using Swarm.Domain.Combat;
using Swarm.Domain.Entities;
using Swarm.Domain.Entities.Weapons;
using Swarm.Domain.Entities.Weapons.Patterns;
using Swarm.Domain.Factories;
using Swarm.Domain.GameObjects;
using Swarm.Domain.GameObjects.Spawners;
using Swarm.Domain.GameObjects.Spawners.Behaviours;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;
using Swarm.Domain.Events;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Common;
using Swarm.Domain.Entities.Projectiles;
using Microsoft.Extensions.Logging.Abstractions;
using Swarm.Domain.Entities.NonPlayerEntities.Behaviours;
using Swarm.Domain.Entities.NonPlayerEntities;
using Swarm.Domain.Entities.NonPlayerEntities.Behaviours.Strategies;
using Swarm.Domain.Factories.Strategies;

namespace Swarm.Application.Services;

public sealed class GameSessionService(
    ILogger<GameSessionService>? logger
) : IGameSessionService
{
    private readonly ILogger<GameSessionService> _logger = logger ?? new NullLogger<GameSessionService>();
    private GameSession? _session;
    private Bounds _stage;
    private readonly List<NonPlayerEntitySpawner> _spawners = [];
    private PlayerArea? _playerArea;
    private TargetArea? _targetArea;
    private Vector2 _crosshairs = new();
    public bool HasSession => _session != null && _playerArea != null && _targetArea != null;

    public void ApplyInput(float dirX, float dirY, float speed)
    {
        if (_session is null) return;
        if (speed <= 0f || (dirX == 0f && dirY == 0f))
        {
            _session.ApplyInput(Direction.From(1, 0), 0f);
            return;
        }
        _session.ApplyInput(Direction.From(dirX, dirY), speed);
    }

    public void Fire(bool isPressed, bool isHeld) => _session?.Fire(isPressed, isHeld);

    public void Reload() => _session?.Reload();

    public GameSnapshot GetSnapshot()
    {
        if (_session is null || _playerArea is null || _targetArea is null)
            throw new DomainException("Game snapshot cannot be created before session and areas are initialized.");

        return DomainMappers.ToSnapshot(_session, _playerArea, _targetArea);
    }

    public void RotateTowards(float targetX, float targetY, float? aimAngle, float AimMagnitude)
    {
        if (_session is null) return;

        if (aimAngle is not null)
        {
            _session.RotateTowardsRadians((float)aimAngle, AimMagnitude);
            return;
        }
        _crosshairs = new Vector2(targetX, targetY);
        _session.RotatePlayerTowards(_crosshairs);
    }

    public void DropBomb() => _session?.DropBomb();

    public async Task StartNewSession(GameSessionConfig config)
    {
        // todo: it is better to have this method take a json string and deserialize it into game session config
        var level = config.LevelConfig;

        var stageConfig = config.StageConfig;

        _stage = new Bounds(
            stageConfig.Left,
            stageConfig.Top,
            stageConfig.Right,
            stageConfig.Bottom
        );

        var playerStart = level.PlayerAreaConfig is AreaConfig playerArea
            ? new Vector2(playerArea.X, playerArea.Y)
            : _stage.Center;
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

        var walls = new List<Wall>(); 
        if (level.WallGeneratorConfig is WallGeneratorConfig wallGeneratorConfig)
        {
            walls = [.. WallFactory.CreateVoronoiWalls(
                start: _stage.TopLeftCorner,
                end: _stage.BottomRightCorner,
                levelBounds: _stage,
                wallRadius: wallGeneratorConfig.WallRadius,
                seedCount: wallGeneratorConfig.WallSeedCount,
                wallDensity: wallGeneratorConfig.WallDensity,
                cellSize: wallGeneratorConfig.WallCellSize,
                minWallCount: GetTotalSpawnerCount(level),
                seed: wallGeneratorConfig.WallRandomSeed
            )];
            
            var spawnerPlacementStrategy = new OpenSideSpawnerStrategy(walls);
            foreach (var wall in walls)
            {
                wall.Spawners = spawnerPlacementStrategy.GetSpawnerPositions(wall, _stage);
            }
        }

        // todo add for walls from config
        var bombs = new List<Bomb>();

        var timer = new RoundTimer(config.RoundLength);

        // todo generate goals from goal config
        // todo hook up goals to session
        _session = new GameSession(EntityId.New(), _stage, player, walls, timer, bombs);

        StartAreas(level);
        StartSpawners(level);

        _logger.LogInformation("New session started");

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

    private void StartAreas(LevelConfig level)
    {
        if (_session is null) return;
        
        if (level.PlayerAreaConfig is AreaConfig playerAreaConfig)
        {
            _playerArea = new PlayerArea(
                _session,
                new Vector2(playerAreaConfig.X, playerAreaConfig.Y),
                new Radius(playerAreaConfig.Radius)
            );
        }

        if (level.TargetAreaConfig is AreaConfig targetAreaConfig)
        {
            _targetArea = new TargetArea(
                _session,
                new Vector2(targetAreaConfig.X, targetAreaConfig.Y),
                new Radius(targetAreaConfig.Radius)
            );
        }
            
    }

    private void StartSpawners(LevelConfig level)
    {
        if (_session is null) return;

        _spawners.Clear();
        var spawnerPositions = _session.SpawnerPoints.ToList();
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

                    // Pick a random position
                    var index = random.Next(spawnerPositions.Count);
                    var spawnPos = spawnerPositions[index];

                    // Remove it from the available pool
                    spawnerPositions.RemoveAt(index);
                    
                    var spawner = new NonPlayerEntitySpawner(
                        _session,
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
                                    dodgeThreshold: dodgeConfig.Threshold
                                );
                            }
                        ),
                        spawnerConfig.BatchSize > 0 ? spawnerConfig.BatchSize : 1
                    );

                    _spawners.Add(spawner);

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
            
            if (healthySpawners is null|| healthySpawners.Count == 0)
                throw new DomainException("Healthy config provided but no healthy spawner defined in non player entity config.");
            
            foreach (var spawnerConfig in healthySpawners)
            {
                if (spawnerPositions.Count == 0) throw new DomainException("No available spawner positions left!");

                // Pick a random position
                var index = random.Next(spawnerPositions.Count);
                var spawnPos = spawnerPositions[index];

                // Remove it from the available pool
                spawnerPositions.RemoveAt(index);
                var batch = spawnerConfig.BatchSize > 0 ? spawnerConfig.BatchSize : 1;
                var spawner = new NonPlayerEntitySpawner(
                    _session,
                    new FixedPositionNonPlayerEntitySpawnerBehaviour(
                        position: spawnPos,
                        cooldownSeconds: spawnerConfig.CooldownSeconds,
                        entityFactory: pos => NonPlayerEntityFactory.CreateHealthy(
                            startPosition: pos,
                            radius: new(entityConfig.Radius),
                            hp: new(entityConfig.HP),
                            speed: entityConfig.Speed,
                            safehouse: _playerArea is null ? _stage.LeftRandomCorner : _playerArea.Position,
                            dodgeThreshold: dodgeConfig.Threshold ?? 150f,
                            infectedSpeed: healthyConfig.InfectedSpeed,
                            infectedTargetThreshold: healthyConfig.InfectedTargetThreshold,
                            infectedDodgeThreshold: healthyConfig.InfectedDodgeThreshold
                        )
                    ),
                    batch
                );

                _spawners.Add(spawner); 

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
            
            var bossWeapon = shooterConfig.Weapon;
            
            var pattern = new SingleShotPattern(
                new Damage(bossWeapon.Damage),
                bossWeapon.ProjectileSpeed,
                new Radius(bossWeapon.ProjectileRadius),
                bossWeapon.ProjectileLifetimeSeconds
            );
            
            var cooldown = new Cooldown(1f / bossWeapon.RatePerSecond);

            var weapon = new Weapon(pattern, cooldown, ProjectileOwnerTypes.Enemy);

            var safehouse = _targetArea is not null
                ? _targetArea.Position
                : _stage.RightRandomCorner;

            foreach (var spawnerConfig in shooterSpawners)
            {
                if (spawnerPositions.Count == 0) throw new DomainException("No available spawner positions left!");

                // Pick a random position
                var index = random.Next(spawnerPositions.Count);
                var spawnPos = spawnerPositions[index];

                // Remove it from the available pool
                spawnerPositions.RemoveAt(index);
                var batch = spawnerConfig.BatchSize > 0 ? spawnerConfig.BatchSize : 1;
                var spawner = new NonPlayerEntitySpawner(
                    _session,
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
                            minionSpawnCount: shooterConfig.MinionSpawnCount ?? 4,
                            minionRadius: new(shooterConfig.MinionSpawnRadius ?? 10f)
                            )
                        ),
                    batch
                );

                _spawners.Add(spawner); 

            }
        }

    }

    public void Pause()
    {
        if (_session is null) return;
        _session.Pause();
        _logger.LogInformation("Session {SessionId} paused", _session.Id);
    }

    public void Resume()
    {
        if (_session is null) return;
        _session.Resume();
        _logger.LogInformation("Session {SessionId} resumed", _session.Id);
    }

    public async Task Restart(GameSessionConfig config)
    {
        if (_session is null)
            throw new InvalidOperationException("No session exists to restart.");

        _logger.LogInformation("Restarting session {SessionId}", _session.Id);

        _session = null;
        _spawners.Clear();
        _playerArea = null;
        _targetArea = null;

        await StartNewSession(config);
    }

    public void Tick(float deltaSeconds)
    {
        if (_session is null) return;

        if (_session.IsPaused || _session.IsTimeUp || _session.IsLevelCompleted || _session.IsInterrupted) return;

        var dt = new DeltaTime(deltaSeconds);
        _playerArea?.Tick(dt);
        _targetArea?.Tick(dt);

        foreach (var spawner in _spawners)
        {
            if (_session.IsWaitingForBombCooldown) continue;
            
            spawner.Tick(dt);
        }

        try
        {
            _session.Tick(dt);
        }
        catch (DomainException)
        {
            if (!_session.IsOverrun) throw;
            _session.Interrupt();
        }

        var events = _session.DomainEvents.ToList();
        _session.ClearDomainEvents();
        foreach (var evt in events)
        {
            HandleDomainEvent(evt);
        }
    }

    private void HandleDomainEvent(IDomainEvent evt)
    {
        // TODO: event contract so presentation can subscribe to it
        switch (evt)
        {
            case LevelCompletedEvent e:
                OnLevelCompletedEvent(e);
                break;
            case TimeIsUpEvent e:
                OnTimeIsUpEvent(e);
                break;
            case TimeUpdatedEvent e:
                OnTimeUpdatedEvent(e);
                break;
            case RadicalSpawnEvent e:
                OnRadicalSpawnEvent(e);
                break;
            case ReachedTargetScoreEvent e:
                OnReachedTargetScoreEvent(e);
                break;
            case HealthyInfectedEvent e:
                OnHealthyInfectedEvent(e);
                break;
            
            default:
                _logger.LogWarning("Unhandled domain event type: {EventType}", evt.GetType().Name);
                break;
        }
    }

    private void OnHealthyInfectedEvent(HealthyInfectedEvent evt)
    {
        if (_session is null) return;

        _logger.LogInformation("Healthy {Id} was infected by a zombie!", evt.Id);

    }

    private void OnReachedTargetScoreEvent(ReachedTargetScoreEvent evt)
    {
        if (_targetArea is null) return;

        _targetArea.OpenToPlayer();
       
    }

    private void OnRadicalSpawnEvent(RadicalSpawnEvent evt)
    {
        if (_session is null) return;

        for (int i = 0; i < evt.SpawnPositions.Count; i++)
        {
            var newEnemy = new Zombie(
                id: EntityId.New(),
                startPosition: evt.SpawnPositions[i],
                radius: new Radius(10f),
                hp: new HitPoints(1),
                behaviour: new SeekBehaviour(
                    speed: 120f,
                    targetStrategy: new PlayerTargetStrategy(),
                    actionStrategy: null,
                    dodgeStrategy: new NearestProjectileDodgeStrategy(
                        owner: ProjectileOwnerTypes.Player,
                        threshold: 250f,
                        multiplier: 3.0f
                    ),
                    runawayStrategy: null
                )
            );

            _session.AddNonPlayerEntity(newEnemy);
        }

        // _logger.LogInformation("{Count} enemies spawned from events!", evt.SpawnPositions.Count);
    }

    private void OnLevelCompletedEvent(LevelCompletedEvent evt)
    {
        _logger.LogInformation("Level completed for session {SessionId}", evt.SessionId);
        _logger.LogInformation("Game saved after reaching target score for session {SessionId}.", evt.SessionId);

    }

    private void OnTimeIsUpEvent(TimeIsUpEvent evt)
    {
        _logger.LogInformation("Time is up for session {SessionId}", evt.SessionId);
    }

    private void OnTimeUpdatedEvent(TimeUpdatedEvent evt)
    {
        // _logger.LogInformation("Session {SessionId} timer: {Seconds} remaining", evt.SessionId, evt.Timer.SecondsRemaining);    
    }

}
