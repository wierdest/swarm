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

        var playerRotation = MathF.Atan2(p.Rotation.Vector.Y, p.Rotation.Vector.X);

        var player = new PlayerDTO(p.Position.X, p.Position.Y, p.Radius.Value, playerRotation);

        var projectiles = new List<ProjectileDTO>(s.Projectiles.Count);
        foreach (var proj in s.Projectiles)
            projectiles.Add(new ProjectileDTO(proj.Position.X, proj.Position.Y, proj.Radius.Value));
        
        var enemies = new List<EnemyDTO>(s.Enemies.Count);
        foreach (var e in s.Enemies)
        {
            var enemyRotation = MathF.Atan2(e.Rotation.Vector.Y, e.Rotation.Vector.X);
            enemies.Add(new EnemyDTO(e.Position.X, e.Position.Y, e.Radius.Value, enemyRotation));
        }

        // This is just a prototype
        var levelStateString = s.IsLevelCompleted ?
        "A descrição do level muda quando chega na TargetArea" :
        "Esse é o level inicial de um protótipo.";

        var hud = new HudDTO(s.Score.Value, p.HP.Value, enemies.Count, levelStateString);

        var walls = new List<DrawableDTO>(s.Walls.Count);
        foreach (var w in s.Walls)
        {
            walls.Add(new DrawableDTO(w.Position.X, w.Position.Y, w.Radius.Value));
        }

        var playerArea = new DrawableDTO(pA.Position.X, pA.Position.Y, pA.Radius.Value);

        var targetArea = new DrawableDTO(t.Position.X, t.Position.Y, t.Radius.Value);

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
