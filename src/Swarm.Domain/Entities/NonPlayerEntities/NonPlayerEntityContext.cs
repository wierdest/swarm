using Swarm.Domain.Primitives;
using Swarm.Domain.Time;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Combat;

namespace Swarm.Domain.Entities.NonPlayerEntities;

public sealed class NonPlayerEntityContext(
    Vector2 enemyPosition,
    Vector2 playerPosition,
    IReadOnlyList<Projectile> projectiles,
    IReadOnlyList<INonPlayerEntity> enemies,
    DeltaTime deltaTime,
    Bounds stage,
    int selfIndex,
    HitPoints hitPoints
)
{
    public Vector2 Position { get; private set; } = enemyPosition;
    public Vector2 PlayerPosition { get; private set; } = playerPosition;
    public IReadOnlyList<Projectile> Projectiles { get; private set; } = projectiles ?? [];
    public IReadOnlyList<INonPlayerEntity> Enemies { get; private set; } = enemies;
    public DeltaTime DeltaTime { get; private set; } = deltaTime;
    public Bounds Stage { get; private set; } = stage;
    public int SelfIndex { get; private set; } = selfIndex;
    public HitPoints HP { get; private set; } = hitPoints;

    public void Update(
        Vector2 enemyPosition,
        Vector2 playerPosition,
        IReadOnlyList<Projectile> projectiles,
        IReadOnlyList<INonPlayerEntity> enemies,
        DeltaTime dt

    )
    {
        Position = enemyPosition;
        PlayerPosition = playerPosition;
        Projectiles = projectiles ?? [];
        Enemies = enemies;
        DeltaTime = dt;
    }
}
