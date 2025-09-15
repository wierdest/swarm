using Swarm.Domain.Primitives;

namespace Swarm.Domain.Entities.Projectiles;

public readonly record struct ProjectileMotionData(Vector2 Position, Direction Direction, float Speed);