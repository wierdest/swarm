using System.Runtime.CompilerServices;
using Swarm.Application.Contracts;
using Swarm.Application.DTOs;
using Swarm.Domain.Entities;
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
        
        var enemies = new List<EnemyDTO>(s.Enemies.Count);
        foreach (var e in s.Enemies)
        {
            var enemyRotation = MathF.Atan2(e.Rotation.Y, e.Rotation.X);
            enemies.Add(new EnemyDTO(e.Position.X, e.Position.Y, e.Radius, enemyRotation));
        }

        // This is just a prototype
        var levelStateString = s.IsLevelCompleted ?
        "A descrição do level muda quando chega na TargetArea" :
        "Esse é o level inicial de um protótipo.";

        levelStateString = s.IsTimeUp ? "TIME IS UP!!!" : levelStateString;

        var hud = new HudDTO(s.Score.Value, p.HP.Value, s.TimeString, enemies.Count, levelStateString);

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
            enemies,
            walls,
            playerArea,
            targetArea
        );
    }

}
