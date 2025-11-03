using Swarm.Domain.Primitives;
using Swarm.Domain.Time;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Combat;

namespace Swarm.Domain.Entities.NonPlayerEntities;

public sealed class NonPlayerEntityContext(
    Vector2 position,
    Vector2 playerPosition,
    IReadOnlyList<Projectile> projectiles,
    IReadOnlyList<INonPlayerEntity> others,
    DeltaTime deltaTime,
    Bounds stage,
    int selfIndex,
    HitPoints hitPoints
)
{
    public Vector2 Position { get; private set; } = position;
    public Vector2 PlayerPosition { get; private set; } = playerPosition;
    public IReadOnlyList<Projectile> Projectiles { get; private set; } = projectiles ?? [];
    public IReadOnlyList<INonPlayerEntity> Others { get; private set; } = others;
    public DeltaTime DeltaTime { get; private set; } = deltaTime;
    public Bounds Stage { get; private set; } = stage;
    public int SelfIndex { get; private set; } = selfIndex;
    public HitPoints HP { get; private set; } = hitPoints;

    public void Update(
        Vector2 position,
        Vector2 playerPosition,
        IReadOnlyList<Projectile> projectiles,
        IReadOnlyList<INonPlayerEntity> others,
        DeltaTime dt

    )
    {
        Position = position;
        PlayerPosition = playerPosition;
        Projectiles = projectiles ?? [];
        Others = others;
        DeltaTime = dt;
    }
}
