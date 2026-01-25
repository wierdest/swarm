using Microsoft.Extensions.Logging;
using Swarm.Application.Contracts;
using Swarm.Application.Config;
using Swarm.Domain.Entities;
using Swarm.Domain.Factories;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;
using Swarm.Domain.Events;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Swarm.Domain.GameObjects.Spawners;
using Swarm.Application.Mappers;
using Swarm.Domain.GameObjects;

namespace Swarm.Application.Services;

public sealed class GameSessionService(
    ILogger<GameSessionService>? logger,
    IGameSessionConfigLoader configLoader
) : IGameSessionService
{
    private readonly ILogger<GameSessionService> _logger = logger ?? new NullLogger<GameSessionService>();
    private readonly IGameSessionConfigLoader _configLoader = configLoader;
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

    public Task StartNewSession(string configJson)
    {
        GameSessionConfig config;
        try
        {
            config = _configLoader.Load(configJson);
        }
        catch (ArgumentException ex)
        {
            throw new DomainException(ex.Message);
        }
        var level = config.LevelConfig;

        _stage = ConfigMappers.ToStage(config);
        var player = ConfigMappers.ToPlayer(config, _stage);
        var walls = ConfigMappers.ToWalls(level, _stage);
        var bombs = ConfigMappers.ToBombs(level);
        var timer = ConfigMappers.ToTimer(config);
        var goal = ConfigMappers.ToGoal(level);

        _session = new GameSession(EntityId.New(), _stage, player, walls, timer, goal, bombs);

        _playerArea = ConfigMappers.ToPlayerArea(_session, level);
        _targetArea = ConfigMappers.ToTargetArea(_session, level);
        _spawners.Clear();
        _spawners.AddRange(ConfigMappers.ToSpawners(_session, level, _playerArea, _targetArea, _stage));

        _logger.LogInformation("New session started");
        return Task.CompletedTask;
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

    public Task Restart(string configJson)
    {
        if (_session is null)
            throw new InvalidOperationException("No session exists to restart.");

        _logger.LogInformation("Restarting session {SessionId}", _session.Id);

        _session = null;
        _spawners.Clear();
        _playerArea = null;
        _targetArea = null;

        return StartNewSession(configJson);
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
            case ZombieSpawnEvent e:
                OnZombieSpawnEvent(e);
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

    private void OnZombieSpawnEvent(ZombieSpawnEvent evt)
    {
        if (_session is null) return;

        for (int i = 0; i < evt.SpawnPositions.Count; i++)
        {
            var newEnemy = NonPlayerEntityFactory.CreateZombie(
                startPosition: evt.SpawnPositions[i],
                radius: evt.Radius,
                hp: evt.HitPoints,
                speed: evt.Speed,
                targetThreshold: evt.TargetThreshold,
                dodgeThreshold: evt.DodgeThreshold,
                dodgeMultiplier: evt.DodgeMultiplier
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
