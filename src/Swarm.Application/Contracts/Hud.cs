using System.Diagnostics;

namespace Swarm.Application.Contracts;

public record class Hud(
    int Score,
    int TargetKills,
    int HP,
    int NumberOfPlayerRespawns,
    string Timer,
    int NumberOfEnemiesAlive,
    string LevelString,
    string WeaponString,
    string BombString
)
{
    public string ToDisplayString()
    {
        var killsText = $"Kill Count: {Score} / {TargetKills}";
        var hpText = $"HP: {HP}";
        var playerDeathsText = $"Deaths: {NumberOfPlayerRespawns}";
        var enemiesText = $"Enemies: {NumberOfEnemiesAlive}";
        var weaponText = WeaponString;
        var timerText = $"Timer: {Timer}";

        return $"{hpText} {playerDeathsText} {weaponText} {killsText} {enemiesText} {timerText}";
    }
}