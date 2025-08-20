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
        var player = new PlayerDTO(p.Position.X, p.Position.Y, p.Radius.Value);

        var list = new List<ProjectileDTO>(s.Projectiles.Count);
        foreach (var proj in s.Projectiles)
            list.Add(new ProjectileDTO(proj.Position.X, proj.Position.Y, proj.Radius.Value));

        return new GameSnapshot(stage, player, list);
    }

}
