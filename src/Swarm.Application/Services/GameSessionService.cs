using Microsoft.Extensions.Logging;
using Swarm.Application.Contracts;
using Swarm.Application.Config;
using Swarm.Application.Primitives;
using Swarm.Domain.Combat;
using Swarm.Domain.Entities;
using Swarm.Domain.Entities.Enemies;
using Swarm.Domain.Entities.Enemies.Behaviours;
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
using Swarm.Domain.Entities.Enemies.DeathTriggers;
using Swarm.Domain.Common;
using Swarm.Domain.Entities.Projectiles;

namespace Swarm.Application.Services;

public sealed class GameSessionService(
    ILogger<GameSessionService> logger,
    IGameSnapshotRepository repository
) : IGameSessionService
{
    private readonly ILogger<GameSessionService> _logger = logger;
    private readonly IGameSnapshotRepository _repository = repository;
    private GameSession? _session;
    private readonly List<EnemySpawner> _spawners = new();
    private PlayerArea? _playerArea;
    private TargetArea? _targetArea;

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
            throw new InvalidOperationException("Game snapshot cannot be created before session and areas are initialized.");

        return DomainMappers.ToSnapshot(_session, _playerArea, _targetArea);
    }

    public void RotateTowards(float targetX, float targetY)
    {
        if (_session is null) return;
        _session.RotatePlayerTowards(new Vector2(targetX, targetY));
    }

    public void StartNewSession(GameConfig config)
    {
        var level = config.LevelConfig;

        var stageConfig = config.StageConfig;

        var stage = new Bounds(
            stageConfig.Left,
            stageConfig.Top,
            stageConfig.Right,
            stageConfig.Bottom
        );

        var playerArea = level.PlayerAreaConfig;
        var playerStart = new Vector2(playerArea.X, playerArea.Y);
        var playerRadius = new Radius(config.PlayerRadius);

        var weaponConfig = level.Weapon;

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
            WeaponTypes.SemiAutomatic
        );

        var player = new Player(
            EntityId.New(),
            playerStart,
            playerRadius,
            playerWeapon,
            weaponConfig.MaxAmmo * 4
            
            );

        var wallsDefs = level.Walls
            .Select(a => (new Vector2(a.X, a.Y), new Radius(a.Radius)));

        var walls = WallFactory.CreateWalls(wallsDefs).ToList();

        var timer = new RoundTimer(config.RoundLength);

        _session = new GameSession(EntityId.New(), stage, player, walls, timer);


        var bossWeapon = new Weapon(pattern, cooldown, ProjectileOwnerTypes.Enemy);

        StartSpawners(level, bossWeapon);
        StartAreas(level);

    }

    private void StartAreas(LevelConfig level)
    {
        if (_session is null) return;

        var playerAreaConfig = level.PlayerAreaConfig;

        _playerArea = new PlayerArea(
            _session,
            new Vector2(playerAreaConfig.X, playerAreaConfig.Y),
            new Radius(level.PlayerAreaConfig.Radius)
        );

        _targetArea = new TargetArea(
            _session,
            new Vector2(level.TargetAreaConfig.X, level.TargetAreaConfig.Y),
            new Radius(level.TargetAreaConfig.Radius)
        );
    }

    private void StartSpawners(LevelConfig level, Weapon weapon)
    {
        if (_session is null) return;

        _spawners.Clear();

        foreach (var spawnerConfig in level.Spawners)
        {
            // TODO refine SpawnerConfig to fully control Spawner Behaviour
            var spawnPos = new Vector2(spawnerConfig.X, spawnerConfig.Y);
            var spawnObjectType = SpawnObjectTypesExtensions.Parse(spawnerConfig.SpawnObjectType);
            var spawner = new EnemySpawner(
                _session,
                new FixedPositionEnemySpawnerBehaviour(
                    position: spawnPos,
                    cooldownSeconds: spawnerConfig.CooldownSeconds,
                    enemyFactory: pos =>
                    {
                        return spawnObjectType switch
                        {
                            SpawnObjectTypes.BasicEnemy => new BasicEnemy(
                                id: EntityId.New(),
                                startPosition: pos,
                                radius: new Radius(10f),
                                initialHitPoints: new HitPoints(1),
                                behaviour: new ChaseBehaviour(speed: 80f)
                            ),

                            SpawnObjectTypes.BossEnemy => new BossEnemy(
                                id: EntityId.New(),
                                startPosition: pos,
                                radius: new Radius(20f),
                                initialHitPoints: new HitPoints(10),
                                behaviour: new PatrolBehaviour(
                                    waypoints: level.BossConfig.Waypoints.Select(p => new Vector2(p.X, p.Y)).ToList(),
                                    speed: level.BossConfig.Speed,
                                    shootRange: level.BossConfig.ShootRange,
                                    shootCooldown: new Cooldown(level.BossConfig.Cooldown)
                                ),
                                weapon: weapon,
                                deathTrigger: new SpawnMinionsDeathTrigger(4, new Radius(10f))
                            ),
                            _ => throw new DomainException($"Invalid spawn object type: {spawnObjectType}")
                        };
                    }
                )
            );

            _spawners.Add(spawner);
        }
    }

    // TODO revise this usage
    public void Stop()
    {
        if (_session is null) return;
        _session.ApplyInput(Direction.From(1, 0), 0f);
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
    
    public void Tick(float deltaSeconds)
    {
        if (_session is null) return;

        if (_session.IsPaused) return;

        var dt = new DeltaTime(deltaSeconds);
        _playerArea?.Tick(dt);
        _targetArea?.Tick(dt);

        foreach (var spawner in _spawners)
        {
            spawner.Tick(dt);
        }

        _session.Tick(dt);

        var events = _session.DomainEvents.ToList();
        _session.ClearDomainEvents();
        foreach (var evt in events)
        {
            HandleDomainEvent(evt);
        }


    }

    private void HandleDomainEvent(IDomainEvent evt)
    {
        switch (evt)
        {
            case LevelCompletedEvent e:
                OnLevelCompleted(e);
                break;
            case TimeIsUpEvent e:
                OnTimeIsUp(e);
                break;
            case TimeUpdatedEvent e:
                OnTimeUpdated(e);
                break;
            case EnemySpawnEvent e:
                SpawnEnemies(e);
                break;
            default:
                _logger.LogWarning("Unhandled domain event type: {EventType}", evt.GetType().Name);
                break;
        }
    }
    
    private void SpawnEnemies(EnemySpawnEvent evt)
    {
        Console.WriteLine("SPAWN ENEMIESSS!");
        if (_session is null) return;

        for (int i = 0; i < evt.SpawnPositions.Count; i++)
        {
            var newEnemy = new BasicEnemy(
                id: EntityId.New(),
                startPosition: evt.SpawnPositions[i],
                radius: new Radius(10f),
                initialHitPoints: new HitPoints(1),
                behaviour: new ChaseBehaviour(speed: 120f)
            );

            _session.AddEnemy(newEnemy);
        }

        _logger.LogInformation("{Count} enemies spawned from events!", evt.SpawnPositions.Count);
    }

    private void OnLevelCompleted(LevelCompletedEvent evt)
    {
        Stop();
        _logger.LogInformation("Level completed for session {SessionId}", evt.SessionId);

    }

    private void OnTimeIsUp(TimeIsUpEvent evt)
    {
        Stop();
        _logger.LogInformation("Time is up for session {SessionId}", evt.SessionId);
    }
    
    private void OnTimeUpdated(TimeUpdatedEvent evt)
    {
        // _logger.LogInformation("Session {SessionId} timer: {Seconds} remaining", evt.SessionId, evt.Timer.SecondsRemaining);    
    }

    public async Task SaveAsync(SaveName saveName, CancellationToken cancellationToken = default)
    {
        if (_session is null || _playerArea is null || _targetArea is null) return;

        var snapshot = DomainMappers.ToSnapshot(_session, _playerArea, _targetArea);
        await _repository.SaveAsync(snapshot, saveName, cancellationToken);
    }

    public async Task<GameSnapshot?> LoadAsync(SaveName saveName, CancellationToken cancellationToken = default)
    {
        return await _repository.LoadAsync(saveName, cancellationToken);
    }
}
