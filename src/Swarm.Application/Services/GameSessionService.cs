using Swarm.Application.Contracts;
using Swarm.Domain.Combat;
using Swarm.Domain.Entities;
using Swarm.Domain.Entities.Behaviours;
using Swarm.Domain.Entities.Patterns;
using Swarm.Domain.Entities.Spawners;
using Swarm.Domain.Entities.Spawners.Behaviours;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Application.Services;

public sealed class GameSessionService : IGameSessionService
{
    private GameSession? _session;
    private EnemySpawner? _spawner;

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
        => _session is null ? default : DomainMappers.ToSnapshot(_session);

    public void RotateTowards(float targetX, float targetY)
    {
        if (_session is null) return;
        _session.RotatePlayerTowards(new Vector2(targetX, targetY));
    }

    public void StartNewSession(StageConfig config)
    {
        var stage = new Bounds(config.Left, config.Top, config.Right, config.Bottom);
        var playerStart = new Vector2(config.PlayerStartX, config.PlayerStartY);
        var playerRadius = new Radius(config.PlayerRadius);

        var pattern = new SingleShotPattern(
            new Damage(config.Weapon.Damage),
            config.Weapon.ProjectileSpeed,
            new Radius(config.Weapon.ProjectileRadius),
            config.Weapon.ProjectileLifetimeSeconds
        );

        var cooldown = new Cooldown(1f / config.Weapon.RatePerSecond);
        var weapon = new Weapon(pattern, cooldown);
        var player = new Player(EntityId.New(), playerStart, playerRadius, weapon);

        _session = new GameSession(stage, player);

        StartSpawner(config);
    }

    private void StartSpawner(StageConfig config)
    {
        if (_session is null) return;
        var spawnPos = new Vector2(config.FixedSpawnPosX, config.FixedSpawnPosY);
        _spawner = new EnemySpawner(
            _session,
            new FixedPositionSpawnerBehaviour(
                position: spawnPos,
                cooldownSeconds: 0.8f,
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

    public void Stop()
    {
        if (_session is null) return;
        _session.ApplyInput(Direction.From(1, 0), 0f);
    }

    public void Tick(float deltaSeconds)
    {
        if (_session is null) return;

        _spawner?.Tick(new DeltaTime(deltaSeconds));
        _session.Tick(new DeltaTime(deltaSeconds));
    }
}
