using System.Data;
using Swarm.Domain.Entities;
using Swarm.Domain.GameObjects;

namespace Swarm.Domain.Factories.Evaluators;

public class BombQuantityEvaluator(
    GameSession session,
    PlayerArea playerArea
)
{
    public int Evaluate()
    {
        var scoreBonus = session.KillBonus;
        var targetScore = session.TargetKills;
        // TODO use these values
        var enemyCount = session.ZombieCount;
        var enemyPopulation = session.ZombiePopulation;
        var bossEnemyCount = session.ShooterCount;
        var playerRespawns = playerArea.PlayerRespawns;
        var bombCount = 0;

        if (scoreBonus.Value > targetScore.Value * 0.20f)
            bombCount++;

        if (playerRespawns <= 1)
            bombCount++;

        return bombCount;
    }
}
