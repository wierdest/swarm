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
    Score targetScore,
    // TODO start from 
    List<Bomb> bombs
)
{
    public EntityId Id { get; } = id;
    public Bounds Stage { get; } = stage;
    public Player Player { get; } = player;
    private readonly List<Projectile> _projectiles = [];
    public IReadOnlyList<Projectile> Projectiles => _projectiles;
    private readonly List<INonPlayerEntity> _nonPlayerEntities = [];
    public IReadOnlyList<INonPlayerEntity> NonPlayerEntities => _nonPlayerEntities;
    public Score EnemyCount => new(_nonPlayerEntities.Count(e => e is Zombie or Shooter));
    public Score EnemyPopulation => new(EnemyCount + _score);
    public Score BossEnemyCount => new(_nonPlayerEntities.Count(e => e is Shooter));
    public bool MaxNonPlayerEntities => NonPlayerEntities.Count >= 666;
    private Score _score = new();
    public Score Score => _score;
    public Score TargetScore => targetScore;
    public Score ScoreBonus => (Score)(HasReachedTargetScore() ? _score - targetScore : 0);
    public bool HasReachedTargetScore() => _score >= TargetScore;
    private readonly List<Bomb> _bombs = bombs;
    public IReadOnlyList<Bomb> Bombs => _bombs;
    public Score BombCount => new(_bombs.Count);
    public void AddBomb(Bomb bomb) => _bombs.Add(bomb);

    private bool _isWaitingForBombCooldown = false;
    public bool IsWaitingForBombCooldown => _isWaitingForBombCooldown;
    public void DropBomb()
    {
        _nonPlayerEntities.ForEach(e => e.Die());
        ((ILivingEntity)Player).Die();
        _bombs.Last().Start();
        _isWaitingForBombCooldown = true;
    }


    public List<Wall> Walls { get; } = walls;
    public IEnumerable<Vector2> SpawnerPoints => Walls.Where(w => w.Spawners is not null).SelectMany(w => w.Spawners!);
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

        if (_isWaitingForBombCooldown)
        {
            var bomb = _bombs[^1];
            bomb.Tick(dt);

            if (bomb.IsReady)
            {
                _isWaitingForBombCooldown = false;
                _bombs.Remove(bomb);
            }
        }
        else
        {
            UpdateTimer(dt);
        }
        
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

       Player.SlideAlongWalls(Walls);
    }

    private void UpdateNonPlayerEntities(DeltaTime dt)
    {
        for (int i = 0; i < _nonPlayerEntities.Count; i++)
        {
            var nonPlayerEntity = _nonPlayerEntities[i];

            var context = new NonPlayerEntityContext<INonPlayerEntity>(
                position: nonPlayerEntity.Position,
                playerPosition: Player.Position, // TODO delegate this decision to???
                projectiles: _projectiles, // pass projectiles because of owner types
                deltaTime: dt,
                selfIndex: i, // id comparison is slow, index comparison is fast, iterating plainlist also cache-ffriendly
                others: _nonPlayerEntities,
                stage: Stage,
                hitPoints: nonPlayerEntity.HP
            );

            nonPlayerEntity.Tick(context);

            UpdateNonPlayerEntities(nonPlayerEntity);

            if (nonPlayerEntity.IsDead)
                continue;

            foreach (var wall in Walls)
            {
                if (nonPlayerEntity.CollidesWith(wall))
                {
                    nonPlayerEntity.RevertLastMovement();
                }
            }

            if (Player.CollidesWith(nonPlayerEntity))
                Player.TakeDamage(new Damage(1));

        }

        _nonPlayerEntities.RemoveAll(e => e.IsDead);
    }

    private void UpdateNonPlayerEntities(INonPlayerEntity enemy)
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

                case RadicalSpawnEvent spawned:
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
    
    public void AddNonPlayerEntity(INonPlayerEntity enemy)
    {
        if (NonPlayerEntities.Count >= 666) return;

        if (enemy != null)
            _nonPlayerEntities.Add(enemy);
    }
}
