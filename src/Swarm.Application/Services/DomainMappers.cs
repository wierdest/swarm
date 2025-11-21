using Swarm.Application.Contracts;
using Swarm.Application.DTOs;
using Swarm.Domain.Entities;
using Swarm.Domain.Entities.NonPlayerEntities;
using Swarm.Domain.Factories;
using Swarm.Domain.Factories.Evaluators;
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

        var enemies = new List<EnemyDTO>(s.NonPlayerEntities.Count);
        foreach (var e in s.NonPlayerEntities)
        {
            var enemyRotation = MathF.Atan2(e.Rotation.Y, e.Rotation.X);
            enemies.Add(new EnemyDTO(e.Position.X, e.Position.Y, e.Radius, enemyRotation, e is Shooter));
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
                enemies,
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

    private static Hud ToHud(GameSession s, PlayerArea pA, TargetArea t)
    {
        var p = s.Player;

        // Level status string
        var levelStateString =
            s.IsLevelCompleted
                ? "SUCCESS!"
                : t.IsOpenToPlayer
                    ? "Reach the green area!"
                    : $"Kill {s.TargetKills} enemies!";

        // Weapon info strin
        // TODO this should come from the player
        var weaponString = "";
        if (p.ActiveWeapon is not null)
        {
            var w = p.ActiveWeapon;
            if (w.CurrentAmmo == 0)
            {
                if (p.Ammo == 0)
                    weaponString = $"{w.Name} [Find some ammo!]";
                else
                    weaponString = $"{w.Name} [Press E to reload] | Fuel: {p.Ammo}";
            }
            else
            {
                weaponString = $"{w.Name} {w.CurrentAmmo}/{w.MaxAmmo} | Fuel: {p.Ammo}";
            }
        }

        var bombString = "[Press Q to drop bomb] | Bombs: " + s.BombCount;

        return new Hud(
            s.Kill,
            s.TargetKills,
            p.HP,
            pA.PlayerRespawns,
            s.TimeString,
            s.ZombieCount,
            levelStateString,
            weaponString,
            bombString,
            s.HealthyCount,
            s.Casualties,
            s.Salvations
        );
    }

   public static SaveGame ToSaveGame(GameSession s, PlayerArea pA, TargetArea t)
   {
        var hud = ToHud(s, pA, t);

        var bombQuantity = new BombQuantityEvaluator(s, pA).Evaluate();
        var bombs = BombFactory.CreateBombs(bombQuantity);
        var bombDTOs = bombs.Select(b => new BombDTO(b.Identifier, b.Cooldown.PeriodSeconds));
        return new SaveGame(
            DateTimeOffset.Now,
            hud,
            bombDTOs
        );
    }
}
