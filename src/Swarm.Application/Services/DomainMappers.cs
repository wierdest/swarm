using Swarm.Application.Contracts;
using Swarm.Application.DTOs;
using Swarm.Domain.Entities;

namespace Swarm.Application.Services;

static class DomainMappers
{
    public static GameSnapshot ToSnapshot(GameSession s)
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

        return new GameSnapshot(stage, player, projectiles, enemies);
    }

}
