using Swarm.Application.Contracts;
using Swarm.Application.DTOs;
using Swarm.Domain.Entities;
using Swarm.Domain.Entities.NonPlayerEntities;
using Swarm.Domain.GameObjects;

namespace Swarm.Application.Services;

static class DomainMappers
{
    public static GameSnapshot ToSnapshot(GameSession s, PlayerArea pA, TargetArea t)
    {
        var stage = new BoundsDTO(s.Stage.Left, s.Stage.Top, s.Stage.Right, s.Stage.Bottom);
        var p = s.Player;

        var playerRotation = MathF.Atan2(p.Rotation.Y, p.Rotation.X);

        var player = new PlayerDTO(p.Position.X, p.Position.Y, p.Radius, playerRotation);

        var projectiles = new List<ProjectileDTO>(s.Projectiles.Count);
        foreach (var proj in s.Projectiles)
            projectiles.Add(new ProjectileDTO(proj.Position.X, proj.Position.Y, proj.Radius));

        var entities = new List<NonPlayerEntityDTO>(s.NonPlayerEntities.Count);
        foreach (var e in s.NonPlayerEntities)
        {
            var rotation = MathF.Atan2(e.Rotation.Y, e.Rotation.X);

            var type = e is Healthy healthy && healthy.IsInfected ? "Zombie" : e.GetType().Name;

            entities.Add(
                new NonPlayerEntityDTO(
                    e.Position.X,
                    e.Position.Y,
                    e.Radius,
                    rotation,
                    type));
        }

        var hud = ToHud(s, pA, t);

        var walls = new List<DrawableDTO>(s.Walls.Count);
        
        foreach (var w in s.Walls)
        {
            walls.Add(new DrawableDTO(w.Position.X, w.Position.Y, w.Radius));
        }

        var playerArea = new DrawableDTO(pA.Position.X, pA.Position.Y, pA.Radius);

        var targetArea = new DrawableDTO(t.Position.X, t.Position.Y, t.Radius);

        return new GameSnapshot(
                stage,
                player,
                hud,
                projectiles,
                entities,
                walls,
                playerArea,
                targetArea,
                s.IsPaused,
                s.IsTimeUp,
                s.IsLevelCompleted,
                s.IsInterrupted,
                t.IsOpenToPlayer,
                s.AimPosition.X,
                s.AimPosition.Y
            );
    }
    private static HudData ToHud(GameSession s, PlayerArea pA, TargetArea t)
    {
        var p = s.Player;
        var w = p.ActiveWeapon;

        return new HudData(
            s.Kills,
            p.HP,
            pA.PlayerRespawns,
            s.TimeString,
            s.EnemyCount + s.InfectedCount,
            s.IsLevelCompleted,
            s.HasReachedTargetGoal(),
            w?.Name ?? string.Empty,
            p.Ammo,
            w?.CurrentAmmo ?? 0,
            w?.MaxAmmo ?? 0,
            s.BombCount,
            s.HealthyCount + s.Salvations,
            s.Casualties,
            s.Salvations,
            s.Infected,
            s.GetGoalDescription()
        );
    }
}
