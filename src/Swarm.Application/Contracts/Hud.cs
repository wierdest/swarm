namespace Swarm.Application.Contracts;

public record class Hud(
    int Score,
    int TargetScore,
    int HP,
    int NumberOfPlayerRespawns,
    string Timer,
    int NumberOfEnemiesAlive,
    string LevelString,
    string WeaponString
)
{
    public string ToDisplayString()
    {
        var scoreText = $"Kill Count: {Score} / {TargetScore}";
        var hpText = $"HP: {HP}";
        var playerDeathsText = $"Deaths: {NumberOfPlayerRespawns}";
        var enemiesText = $"Enemies: {NumberOfEnemiesAlive}";
        var weaponText = WeaponString;
        var timerText = $"Timer: {Timer}";

        return $"{hpText} {playerDeathsText} {weaponText} {scoreText} {enemiesText} {timerText}";
    }
}