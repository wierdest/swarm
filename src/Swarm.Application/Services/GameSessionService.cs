using Microsoft.Extensions.Logging;
using Swarm.Application.Contracts;
using Swarm.Application.Config;
using Swarm.Application.Primitives;
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
using Swarm.Domain.Entities.NonPlayerEntities.DeathTriggers;
using Swarm.Domain.Factories.Strategies;
using Swarm.Application.DTOs;
using System.Linq;
using Swarm.Domain.Factories.Evaluators;

namespace Swarm.Application.Services;

public sealed class GameSessionService(
    ILogger<GameSessionService>? logger,
    ISaveGameRepository repository
) : IGameSessionService
{
    private readonly ILogger<GameSessionService> _logger = logger ?? new NullLogger<GameSessionService>();
    private readonly ISaveGameRepository _repository = repository;
    private GameSession? _session;
    private readonly List<NonPlayerEntitySpawner> _spawners = new();
    private PlayerArea? _playerArea;
    private TargetArea? _targetArea; 
    private readonly object _savesLock = new();
    private List<SaveGame> _allSaves = [];
    private Vector2 _crosshairs = new();

    private readonly static int FINAL_TARGET_SCORE = 800;
    private readonly static int TARGET_SCORE_INCREMENT = 125;

    private bool _hasReachedAlpha = false;


    // thread safe, because saving may be done in multithreading later on???
    private IReadOnlyList<SaveGame> AllSaves
    {
        get
        {
            lock (_savesLock)
            {
                return _allSaves.AsReadOnly();
            }
        }
    }

    public IReadOnlyList<SaveGame> GetSaveGames() => AllSaves;

    public SaveGame? LatestCachedSave
    {
        get
        {
            lock (_savesLock)
            {
                return _allSaves.Count > 0 ? _allSaves[0] : null;
            }
        }
    }

    public async Task LoadAllSavesAsync(SaveName saveName, CancellationToken cancellationToken = default)
    {
        var saves = await _repository.LoadAllAsync(saveName, cancellationToken);
        lock (_savesLock)
        {
            _allSaves = [.. saves];
        }
    }

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
            throw new InvalidOperationException("Game snapshot cannot be created before session and areas are initialized.");

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

    public async Task StartNewSession(GameConfig config)
    {
        // TODO Domain mapper deals with this:
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
            WeaponTypes.Automatic
        );

        var player = new Player(
            EntityId.New(),
            playerStart,
            playerRadius,
            playerWeapon,
            weaponConfig.MaxAmmo * 4
        );

        // Loads narrative! 
        await LoadAllSavesAsync(new SaveName("Progression"));

        int targetScoreValue = LatestCachedSave?.Hud.TargetKills is int prevTarget
            ? GetLevelTargetScore(prevTarget)
            : level.InitialTargetScore;

        
        var targetScore = new Score(targetScoreValue);

        var phaseALevel = targetScore.Value.Equals(FINAL_TARGET_SCORE);

        _logger.LogInformation("Phase A Level? {}", phaseALevel.ToString());

        if (!_hasReachedAlpha && phaseALevel)
        {
            targetScore = new Score(level.InitialTargetScore);
            _hasReachedAlpha = true;
        }

        var walls = WallFactory.CreateVoronoiWalls(
            start: playerStart,
            end: new Vector2(level.TargetAreaConfig.X, level.TargetAreaConfig.Y),
            levelBounds: stage,
            wallRadius: 40f,
            seedCount: phaseALevel ? 7 : 3
 
        ).ToList();

        var spawnerPlacementStrategy = new OpenSideSpawnerStrategy(walls);
        foreach (var wall in walls)
        {
            wall.Spawners = spawnerPlacementStrategy.GetSpawnerPositions(wall, stage);
        }
        var timer = new RoundTimer(config.RoundLength);

        var bombs = LatestCachedSave?.Bombs
            .Select(b => new Bomb(b.Identifier, new Cooldown(b.CooldownSeconds)))
            .ToList()
            ?? [];

        
        _session = new GameSession(EntityId.New(), stage, player, walls, timer, targetScore, bombs);

        StartSpawners(level);
        StartAreas(level);

        _logger.LogInformation("New session started with target score: {TargetScore}", targetScore.Value);

    }

    private static int GetLevelTargetScore(int prevTarget)
    {
        var next = prevTarget + TARGET_SCORE_INCREMENT;

        return next > FINAL_TARGET_SCORE ? FINAL_TARGET_SCORE : next;
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

    private void StartSpawners(LevelConfig level)
    {
        if (_session is null) return;

        var boss = level.BossConfig;

        var pattern = new SingleShotPattern(
            new Damage(boss.Damage),
            boss.ProjectileSpeed,
            new Radius(boss.ProjectileRadius),
            boss.ProjectileLifetimeSeconds
        );

        var cooldown = new Cooldown(1f / boss.ProjectileRatePerSecond);

        var weapon = new Weapon(pattern, cooldown, ProjectileOwnerTypes.Enemy);

        _spawners.Clear();

        var spawnerPositions = _session.SpawnerPoints.ToList();
        var random = new Random();

        foreach (var spawnerConfig in level.Spawners)
        {
            if (spawnerPositions.Count == 0)
                throw new DomainException("No available spawner positions left!");

            // Pick a random position
            var index = random.Next(spawnerPositions.Count);
            var spawnPos = spawnerPositions[index];

            // Remove it from the available pool
            spawnerPositions.RemoveAt(index);

            var spawnObjectType = SpawnObjectTypesExtensions.Parse(spawnerConfig.SpawnObjectType);

            // TODO behaviour factory
            var patrolBehaviour = new PatrolBehaviour(
                                    waypoints: level.BossConfig.Waypoints.Select(p => new Vector2(p.X, p.Y)).ToList(),
                                    speed: level.BossConfig.Speed,
                                    actionStrategy: new RangeShootStrategy(
                                        shootRange: level.BossConfig.ShootRange
                                    ),
                                    actionCooldown: new Cooldown(level.BossConfig.Cooldown),
                                    dodgeStrategy: new NearestProjectileDodgeStrategy(
                                        owner: ProjectileOwnerTypes.Player,
                                        threshold: 150f,
                                        multiplier: 1.5f
                                    ),
                                    runawayStrategy: null
                                );

            var chaseBehaviour = new SeekBehaviour(
                                    speed: 200f,
                                    targetStrategy: new PlayerTargetStrategy(),
                                    actionStrategy: null,
                                    dodgeStrategy: new NearestProjectileDodgeStrategy(
                                        owner: ProjectileOwnerTypes.Player,
                                        threshold: 150f
                                    ),
                                    runawayStrategy: null
                                );

            var chaseShootBehaviour = new SeekBehaviour(
                                    speed: 200f,
                                    targetStrategy: new PlayerTargetStrategy(),
                                    actionStrategy: new RangeShootStrategy(
                                        shootRange: level.BossConfig.ShootRange
                                    ),
                                    dodgeStrategy: new NearestProjectileDodgeStrategy(
                                        owner: ProjectileOwnerTypes.Player,
                                        threshold: 150f
                                    ),
                                    runawayStrategy: new SafehouseRunawayStrategy(
                                        hitPointsThreshold: 9,
                                        safeHouse: new Vector2(level.TargetAreaConfig.X + 12f, level.TargetAreaConfig.Y - 12f),
                                        safeHouseWeight: 0.5f,
                                        avoidPlayerWeight: 0.5f
                                    )
                                );

            var spawner = new NonPlayerEntitySpawner(
                _session,
                new FixedPositionNonPlayerEntitySpawnerBehaviour(
                    position: spawnPos,
                    cooldownSeconds: spawnerConfig.CooldownSeconds,
                    entityFactory: pos =>
                    {
                        return spawnObjectType switch
                        {
                            SpawnObjectTypes.Zombie => new Zombie(
                                id: EntityId.New(),
                                startPosition: pos,
                                radius: new Radius(10f),
                                hp: new HitPoints(1),
                                behaviour: chaseBehaviour
                            ),

                            SpawnObjectTypes.Shooter => new Shooter(
                                id: EntityId.New(),
                                startPosition: pos,
                                radius: new Radius(12f),
                                hp: new HitPoints(10),
                                behaviour: chaseShootBehaviour,
                                weapon: weapon,
                                deathTrigger: new SpawnRadicalsDeathTrigger(4, new Radius(10f))
                            ),
                            _ => throw new DomainException($"Invalid spawn object type: {spawnObjectType}")
                        };
                    }
                ),
                spawnerConfig.BatchSize
            );

            _spawners.Add(spawner);
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

    public async Task Restart(GameConfig config)
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
            case RadicalSpawnEvent e:
                OnEnemySpawn(e);
                break;
            case ReachedTargetScoreEvent e:
                OnReachedTargetScoreEvent(e);
                break;
            default:
                _logger.LogWarning("Unhandled domain event type: {EventType}", evt.GetType().Name);
                break;
        }
    }

    private void OnReachedTargetScoreEvent(ReachedTargetScoreEvent evt)
    {
        if (_targetArea is null) return;

        _targetArea.OpenToPlayer();
       
    }

    private void OnEnemySpawn(RadicalSpawnEvent evt)
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

    private void OnLevelCompleted(LevelCompletedEvent evt)
    {
        _logger.LogInformation("Level completed for session {SessionId}", evt.SessionId);
         _ = SaveAsync(new SaveName("Progression"));
        _logger.LogInformation("Game saved after reaching target score for session {SessionId}.", evt.SessionId);

    }

    private void OnTimeIsUp(TimeIsUpEvent evt)
    {
        _ = SaveAsync(new SaveName("Progression"));        
        _logger.LogInformation("Time is up for session {SessionId}", evt.SessionId);
    }

    private void OnTimeUpdated(TimeUpdatedEvent evt)
    {
        // _logger.LogInformation("Session {SessionId} timer: {Seconds} remaining", evt.SessionId, evt.Timer.SecondsRemaining);    
    }

    public async Task SaveAsync(SaveName saveName, CancellationToken cancellationToken = default)
    {
        if (_session is null || _playerArea is null || _targetArea is null) return;

        var save = DomainMappers.ToSaveGame(_session, _playerArea, _targetArea);

        await _repository.SaveAsync(save, saveName, cancellationToken);
    }

}
