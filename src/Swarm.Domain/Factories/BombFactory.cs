using Swarm.Domain.Combat;
using Swarm.Domain.Time;

namespace Swarm.Domain.Factories;
public static class BombFactory
{
    public static Bomb[] CreateBombs(
       int bombCount
    )
    {
        var bombs = new Bomb[bombCount];
        for (int i = 0; i < bombCount; i++)
        {
            // TODO evaluate for bomb type
            bombs[i] = new Bomb("A-Bomb", new Cooldown(2.0f));
        }

        return bombs;
    }
}
