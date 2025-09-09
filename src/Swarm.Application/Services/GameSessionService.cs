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

namespace Swarm.Application.Services;

public sealed class GameSessionService(
    ILogger<GameSessionService> logger,
    IGameSnapshotRepository repository
) : IGameSessionService
{
    private readonly ILogger<GameSessionService> _logger = logger;
    private readonly IGameSnapshotRepository _repository = repository;
    private GameSession? _session;
    private EnemySpawner? _spawner;
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

    public void Fire() => _session?.Fire();

    public GameSnapshot GetSnapshot()
        => _session is null || _playerArea is null || _targetArea is null ? default : DomainMappers.ToSnapshot(
            _session,
            _playerArea,
            _targetArea
            );


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

        var weapon = new Weapon(pattern, cooldown);

        var player = new Player(EntityId.New(), playerStart, playerRadius, weapon);

        var wallsDefs = level.Walls
            .Select(a => (new Vector2(a.X, a.Y), new Radius(a.Radius)));

        var walls = WallFactory.CreateWalls(wallsDefs).ToList();

        var timer = new RoundTimer(config.RoundLength);

        _session = new GameSession(EntityId.New(), stage, player, walls, timer);

        // Subscribe to events!
        _session.LevelCompleted += OnLevelCompleted;
        _session.TimeIsUp += OnTimeIsUp;
        _session.TimeUpdated += OnTimeUpdated;

        StartSpawner(level);
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

    private void StartSpawner(LevelConfig level)
    {
        if (_session is null) return;

        foreach (var spawnerConfig in level.Spawners)
        {
            // TODO refine SpawnerConfig to fully control Spawner Behaviour
            var spawnPos = new Vector2(spawnerConfig.X, spawnerConfig.Y);
            _spawner = new EnemySpawner(
                _session,
                new FixedPositionEnemySpawnerBehaviour(
                    position: spawnPos,
                    cooldownSeconds: spawnerConfig.CooldownSeconds,
                    enemyFactory: pos =>
                        new BasicEnemy(
                            id: EntityId.New(),
                            startPosition: pos,
                            radius: new Radius(10f),
                            initialHitPoints: new HitPoints(1),
                            behaviour: new ChaseBehaviour(speed: 80f)
                        )
                )
            );
        }
    }

    public void Stop()
    {
        if (_session is null) return;
        _session.ApplyInput(Direction.From(1, 0), 0f);
    }

    public void Tick(float deltaSeconds)
    {
        if (_session is null) return;
        var dt = new DeltaTime(deltaSeconds);
        _playerArea?.Tick(dt);
        _targetArea?.Tick(dt);
        _spawner?.Tick(dt);
        _session.Tick(dt);
    }

    private void OnLevelCompleted(GameSession session)
    {
        Stop();
        _logger.LogInformation("Level completed for session {SessionId}", session.Id);

    }

    private void OnTimeIsUp(GameSession session)
    {
        Stop();
        _logger.LogInformation("Time is up for session {SessionId}", session.Id);
    }
    
    private void OnTimeUpdated(GameSession session, RoundTimer secondsRemaining)
    {
        
        // _logger.LogInformation("Session {SessionId} timer: {Seconds} seconds remaining", session.Id, secondsRemaining);
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
