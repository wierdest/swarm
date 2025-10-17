using Swarm.Domain.Combat;
using Swarm.Domain.Entities.NonPlayerEntities;
using Swarm.Domain.Entities.Projectiles;
using Swarm.Domain.Events;
using Swarm.Domain.GameObjects;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Primitives;
using Swarm.Domain.Time;

namespace Swarm.Domain.Entities;

public sealed class GameSession(
    EntityId id,
    Bounds stage,
    Player player,
    List<Wall> walls,
    RoundTimer timer,
    Score targetScore
)
{
    public EntityId Id { get; } = id;
    public Bounds Stage { get; } = stage;
    public Player Player { get; } = player;
    private readonly List<Projectile> _projectiles = [];
    public IReadOnlyList<Projectile> Projectiles => _projectiles;
    private readonly List<INonPlayerEntity> _nonPlayerEntities = [];
    public IReadOnlyList<INonPlayerEntity> NonPlayerEntities => _nonPlayerEntities;
    public int EnemyCount => _nonPlayerEntities.Count(e => e is BasicEnemy or BossEnemy);
    private Score _score = new();
    public Score Score => _score;   
    public Score TargetScore => targetScore;
    public bool HasReachedTargetScore() => _score == TargetScore;
    public List<Wall> Walls { get; } = walls;
    private bool _isLevelCompleted = false;
    public bool IsLevelCompleted => _isLevelCompleted;
    private RoundTimer _timer = timer;
    private float _accumulator = 0f;
    private bool _isTimeUp = false;
    public bool IsTimeUp => _isTimeUp;
    public String TimeString => _timer.ToString();
    private bool _isPaused;
    public bool IsPaused => _isPaused;
    public void Pause() => _isPaused = true;
    public void Resume() => _isPaused = false;
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
    private void RaiseEvent(IDomainEvent evt) => _domainEvents.Add(evt);
    public void ClearDomainEvents() => _domainEvents.Clear();
    public bool IsOverrun => NonPlayerEntities.Count > TargetScore;
    private bool _isInterrupted = false;
    public bool IsInterrupted => _isInterrupted;
    public void Interrupt() => _isInterrupted = true;
    public Vector2 AimPosition = new();
    public void CompleteLevel()
    {
        if (_isLevelCompleted)
            return;

        _isLevelCompleted = true;

        RaiseEvent(new LevelCompletedEvent(Id));
    }

    public void ApplyInput(Direction dir, float speed) =>
        Player.ApplyInput(dir, speed);

    public void Fire(bool isPressed, bool isHeld)
    {
        var weapon = Player.ActiveWeapon;

        if (weapon.IsAutomatic() && !isHeld) return;

        if (!weapon.IsAutomatic() && !isPressed) return;

        if (Player.TryFire(out var projectiles))
            _projectiles.AddRange(projectiles);
    }

    public void Reload()
    {
        Player.ReloadWeapon();
    }

    public void AddAmmo(int value)
    {
        Player.AddAmmo(value);
    }

    public void RotatePlayerTowards(Vector2 target)
    {
        AimPosition = target;
        Player.RotateTowards(target);
    }

    public void RotateTowardsRadians(float aimAngleRadians, float aimMagnitude)
    {
        var dir = new Vector2(
            (float)Math.Cos(aimAngleRadians),
            (float)Math.Sin(aimAngleRadians)
        );

        // This will eventually come from a conffig
        const float aimRadius = 200f;
        var center = Player.Position + new Vector2(Player.Radius.Value, Player.Radius.Value);

        // Compute aim target around player
        AimPosition = center + dir * (aimRadius * aimMagnitude);

        RotatePlayerTowards(AimPosition);
    }


    public void Tick(DeltaTime dt)
    {
        if (_isPaused || _isTimeUp || _isLevelCompleted) return;

        UpdateTimer(dt);
        UpdatePlayer(dt);
        UpdateNonPlayerEntities(dt);
        UpdateProjectiles(dt);
    }

    private void UpdateTimer(DeltaTime dt)
    {
        _accumulator += dt;
        if (_accumulator >= 1f)
        {
            int secondsPassed = (int)_accumulator;
            _accumulator -= secondsPassed;

            _timer = _timer.Tick(secondsPassed);

            if (_timer.IsExpired && !_isTimeUp)
            {
                _isTimeUp = true;
                RaiseEvent(new TimeIsUpEvent(Id));
            }
            else
            {
                RaiseEvent(new TimeUpdatedEvent(Id, _timer));
            }

        }
    }

    private void UpdatePlayer(DeltaTime dt)
    {
        Player.Tick(dt, Stage);

        foreach (var wall in Walls)
        {
            if (Player.CollidesWith(wall))
            {
                Player.RevertLastMovement();
            }
        }
    }

    private void UpdateNonPlayerEntities(DeltaTime dt)
    {
        for (int i = 0; i < _nonPlayerEntities.Count; i++)
        {
            var enemy = _nonPlayerEntities[i];

            var context = new NonPlayerEntityContext(
                enemyPosition: enemy.Position,
                playerPosition: Player.Position,
                projectiles: _projectiles, // pass projectiles because of owner types
                deltaTime: dt,
                selfIndex: i, // id comparison is slow, index comparison is fast, iterating plainlist also cache-ffriendly
                enemies: _nonPlayerEntities,
                stage: Stage,
                hitPoints: enemy.HP
            );

            enemy.Tick(context);

            UpdateEnemyEvents(enemy);

            if (enemy.IsDead)
                continue;


            foreach (var wall in Walls)
            {
                if (enemy.CollidesWith(wall))
                {
                    enemy.RevertLastMovement();
                }
            }

            if (Player.CollidesWith(enemy))
                Player.TakeDamage(new Damage(1));

        }

        _nonPlayerEntities.RemoveAll(e => e.IsDead);
    }

    private void UpdateEnemyEvents(INonPlayerEntity enemy)
    {
        if (enemy.DomainEvents is null) return;

        foreach (var evt in enemy.DomainEvents)
        {
            switch (evt)
            {
                case EnemyFiredEvent fired:
                    // here you translate intent -> add projectile(s) from the active weapon
                    _projectiles.AddRange(fired.Projectiles);
                    break;

                case EnemySpawnEvent spawned:
                    RaiseEvent(spawned);
                    break;
            }
        }
        enemy.ClearDomainEvents();
            
    }

    private void UpdateProjectiles(DeltaTime dt)
    {
        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            var projectile = _projectiles[i];
            projectile.Tick(dt);

            if (IsProjectileExpired(projectile) || HandleProjectileCollisions(projectile))
            {
                _projectiles.RemoveAt(i);
            }
        }
    }

    private bool IsProjectileExpired(Projectile projectile) => projectile.IsExpired || !Stage.Contains(projectile.Position);

    private bool HandleProjectileCollisions(Projectile projectile)
    {
        foreach (var wall in Walls)
        {
            if (projectile.CollidesWith(wall))
                return true;
        }

        if (projectile.Owner == ProjectileOwnerTypes.Player)
        {
            foreach (var enemy in _nonPlayerEntities)
            {
                if (enemy.IsDead)
                    continue;

                if (projectile.CollidesWith(enemy))
                {
                    enemy.TakeDamage(projectile.Damage);
                    if (enemy.IsDead)
                    {
                        _score += 1;
                        if (HasReachedTargetScore())
                        {
                            RaiseEvent(new ReachedTargetScoreEvent(Id));
                        }
                    }
              
                    return true;
                }
            }
        }
        else if (
            projectile.Owner == ProjectileOwnerTypes.Enemy &&
            projectile.CollidesWith(Player))
        {

            Player.TakeDamage(projectile.Damage);
            return true;
            
        }
        return false;
    }
    
    public void AddEnemy(INonPlayerEntity enemy)
    {
        if (NonPlayerEntities.Count >= 666) return;

        if (enemy != null)
            _nonPlayerEntities.Add(enemy);
    }
}
