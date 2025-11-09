using Swarm.Domain.Primitives;
using Swarm.Domain.Time;
using Swarm.Domain.Interfaces;
using Swarm.Domain.Combat;

namespace Swarm.Domain.Entities.NonPlayerEntities;

public sealed class NonPlayerEntityContext<T>(
    Vector2 position,
    Vector2 playerPosition,
    IReadOnlyList<Projectile> projectiles,
    IReadOnlyList<INonPlayerEntity> others,
    DeltaTime deltaTime,
    Bounds stage,
    int selfIndex,
    HitPoints hitPoints
) where T : INonPlayerEntity
{
    public Vector2 Position { get; private set; } = position;
    public Vector2 PlayerPosition { get; private set; } = playerPosition;
    public IReadOnlyList<Projectile> Projectiles { get; private set; } = projectiles ?? [];
    public IReadOnlyList<INonPlayerEntity> Others { get; private set; } = others;
    public DeltaTime DeltaTime { get; private set; } = deltaTime;
    public Bounds Stage { get; private set; } = stage;
    public int SelfIndex { get; private set; } = selfIndex;
    public HitPoints HP { get; private set; } = hitPoints;
    public T Self => (T)Others[SelfIndex];
    public INonPlayerEntity? NearestPerson => FindNearestOfType<Healthy>();
    private INonPlayerEntity? FindNearestOfType<TType>() where TType : INonPlayerEntity
    {
        INonPlayerEntity? nearest = null;
        float bestDistSq = float.MaxValue;
        var selfPos = Position;

        foreach (var other in Others)
        {
            if (other is not TType target || ReferenceEquals(other, Self) || target.IsDead)
                continue;

            var delta = target.Position - selfPos;
            float distSq = delta.LengthSquared();

            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                nearest = target;
            }
        }

        return nearest;
    }
}
